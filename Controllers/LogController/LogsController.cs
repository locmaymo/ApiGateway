using ApiGateway.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Globalization;
using System.Text.Json;

namespace ApiGateway.Controllers
{
    public class LogsController : Controller
    {
        private readonly ElasticsearchClient _elasticClient;

        public LogsController(ElasticsearchClient elasticClient, ILogger<LogsController> logger)
        {
            _elasticClient = elasticClient;
        }   

        public IActionResult Index()
        {
            return View();
        }

        // GET: /Logs/Fields get all fields in the index
        [HttpGet("api/logs/fields")]
        public async Task<IActionResult> GetLogFields()
        {
            try
            {
                // Replace "your-index-name" with the actual name of your index
                var response = await _elasticClient.Indices.GetMappingAsync(new GetMappingRequest("application-logs-emails-api-development-*"));

                if (!response.IsValidResponse || !response.Indices.Any())
                {
                    return StatusCode(500, "Failed to retrieve index mapping.");
                }

                // Assuming you have a single index
                var indexMapping = response.Indices.First().Value;

                // Get the properties dictionary
                var properties = indexMapping.Mappings.Properties;

                // Flatten the properties to get all field paths
                var fields = GetAllFields(properties);

                return Ok(fields);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Helper method to flatten properties and get all field paths
        private List<string> GetAllFields(Properties properties, string parentPath = "")
        {
            var fields = new List<string>();

            foreach (var prop in properties)
            {
                var fieldName = parentPath + prop.Key;

                if (prop.Value is ObjectProperty objectProp && objectProp.Properties != null)
                {
                    // Recursively get nested fields
                    var nestedFields = GetAllFields(objectProp.Properties, fieldName + ".");
                    fields.AddRange(nestedFields);
                }
                else
                {
                    fields.Add(fieldName);
                }
            }

            return fields;
        }

        // POST: /Logs/Search get all log the index
        [HttpPost("api/logs/search")]
        public async Task<IActionResult> SearchLogs([FromBody] LogSearchRequest request)
        {
            try
            {
                var mustQueries = new List<Query>();
                var mustNotQueries = new List<Query>();

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    mustQueries.Add(new SimpleQueryStringQuery
                    {
                        Query = request.SearchTerm + "*"
                    });
                }

                // Date range filter
                if (!string.IsNullOrEmpty(request.StartDate) || !string.IsNullOrEmpty(request.EndDate))
                {
                    var dateRangeQuery = new DateRangeQuery(field: "@timestamp");

                    // Adjusted format string for parsing
                    var format = "dd-MM-yyyy HH:mm:ss";
                    var culture = CultureInfo.InvariantCulture;

                    if (DateTime.TryParseExact(request.StartDate, format, culture, DateTimeStyles.None, out var startDate))
                    {
                        // Convert to UTC if necessary
                        dateRangeQuery.Gte = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
                    }
                    if (DateTime.TryParseExact(request.EndDate, format, culture, DateTimeStyles.None, out var endDate))
                    {
                        // Convert to UTC if necessary
                        dateRangeQuery.Lte = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();
                    }

                    mustQueries.Add(dateRangeQuery);
                }

                // Xử lý Filters
                if (request.Filters != null && request.Filters.Any())
                {
                    foreach (var filter in request.Filters)
                    {
                        switch (filter.Operator.ToLower())
                        {
                            case "exists":
                                mustQueries.Add(new ExistsQuery
                                {
                                    Field = filter.Field
                                });
                                break;

                            case "not_exists":
                                mustNotQueries.Add(new ExistsQuery
                                {
                                    Field = filter.Field
                                });
                                break;

                            case "equals":
                                mustQueries.Add(new TermQuery(field: filter.Field)
                                {
                                    Value = filter.Value
                                });
                                break;

                            case "not_equals":
                                // Ensure the field exists
                                mustQueries.Add(new ExistsQuery
                                {
                                    Field = filter.Field
                                });

                                // Exclude documents where the field equals the specified value
                                mustNotQueries.Add(new TermQuery(field: filter.Field)
                                {
                                    Value = filter.Value
                                });
                                break;

                            case "contains":
                                mustQueries.Add(new MatchQuery(field: filter.Field)
                                {
                                    Query = filter.Value
                                });
                                break;

                            case "not_contains":
                                // Ensure the field exists
                                mustQueries.Add(new ExistsQuery
                                {
                                    Field = filter.Field
                                });

                                // Exclude documents where the field matches the specified value
                                mustNotQueries.Add(new MatchQuery(field: filter.Field)
                                {
                                    Query = filter.Value
                                });
                                break;

                            default:
                                throw new NotSupportedException($"Operator '{filter.Operator}' is not supported.");
                        }
                    }
                }

                var searchRequest = new SearchRequest<LogEvent>
                {
                    Query = new BoolQuery
                    {
                        Must = mustQueries,
                        MustNot = mustNotQueries
                    },
                    Sort = new[]
                    {
                        SortOptions.Field("@timestamp", new FieldSort
                        {
                            Order = SortOrder.Desc
                        })
                    },
                    Size = request.Size,
                    From = request.From
                };

                var response = await _elasticClient.SearchAsync<LogEvent>(searchRequest);

                if (!response.IsValidResponse)
                {
                    return StatusCode(500, response.ElasticsearchServerError?.Error?.Reason ?? "Unknown error");
                }

                var result = new
                {
                    // lấy tổng kết quả
                    total = response.Hits.Count,
                    logs = response.Documents
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        // get log by user id param
        [HttpGet("api/logs")]
        public async Task<IActionResult> GetLogs(
            [FromQuery] int? userid,
            [FromQuery] string key,
            [FromQuery] int size = 100,
            [FromQuery(Name = "startTime")] string startTime = null,
            [FromQuery(Name = "endTime")] string endTime = null)
        {
            try
            {
                var mustQueries = new List<Query>();

                // Lọc theo UserId nếu được cung cấp
                if (userid.HasValue)
                {
                    mustQueries.Add(new TermQuery("fields.UserId")
                    {
                        Value = userid.Value
                    });
                }

                // Nếu 'key' được cung cấp, chỉ lọc trên 'fields.LogType.raw'
                if (!string.IsNullOrEmpty(key))
                {
                    mustQueries.Add(new TermQuery("fields.LogType.raw")
                    {
                        Value = key
                    });
                }

                // Lọc theo khoảng thời gian
                if (!string.IsNullOrEmpty(startTime) || !string.IsNullOrEmpty(endTime))
                {
                    var dateRangeQuery = new DateRangeQuery("@timestamp");

                    // Định dạng thời gian, ví dụ: "yyyy-MM-ddTHH:mm:ss"
                    var format = "yyyy-MM-ddTHH:mm:ss";
                    var culture = CultureInfo.InvariantCulture;

                    if (DateTime.TryParseExact(startTime, format, culture, DateTimeStyles.None, out var startDate))
                    {
                        dateRangeQuery.Gte = startDate;
                    }

                    if (DateTime.TryParseExact(endTime, format, culture, DateTimeStyles.None, out var endDate))
                    {
                        dateRangeQuery.Lte = endDate;
                    }
                    mustQueries.Add(dateRangeQuery);
                }

                var searchRequest = new SearchRequest<LogEvent>("application-logs-emails-api-development-*")
                {
                    Query = new BoolQuery
                    {
                        Must = mustQueries.ToArray()
                    },
                    Sort = new[]
                    {
                        SortOptions.Field("@timestamp", new FieldSort
                        {
                            Order = SortOrder.Desc
                        })
                    },

                    Size = size,
                    From = 0 // Bạn có thể thêm tham số `From` nếu cần phân trang
                };

               
                var response = await _elasticClient.SearchAsync<LogEvent>(searchRequest);

                if (!response.IsValidResponse)
                {
                    return StatusCode(500, response.ElasticsearchServerError?.Error?.Reason ?? "Unknown error");
                }

                var result = new
                {
                    total = response.Total,
                    logs = response.Documents
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    


    [HttpGet("stats/daily")]
        public async Task<IActionResult> GetDailyLogStats()
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<LogEvent>(s => s
                    .Index("applogs-stocks-api") // Đặt tên chỉ mục đúng
                    .Size(0) // Không cần lấy tài liệu
                    .Aggregations(a => a
                        .Add("logs_per_day", agg => agg
                            .DateHistogram(dh => dh
                                .Field(f => f.Timestamp) // Thay 'Timestamp' bằng tên trường thực tế trong LogEvent
                                .CalendarInterval(CalendarInterval.Day) // Sử dụng enum thay vì chuỗi
                                .Format("yyyy-MM-dd")
                            )
                        )
                    )
                );

                if (!searchResponse.IsValidResponse)
                {
                    return StatusCode(500, searchResponse.ElasticsearchServerError?.Error?.Reason ?? "Unknown error");
                }

                // Lấy kết quả của aggregations sử dụng GetDateHistogram
                var histogram = searchResponse.Aggregations.GetDateHistogram("logs_per_day");

                var labels = new List<string>();
                var data = new List<long>();

                foreach (var bucket in histogram.Buckets)
                {
                    labels.Add(bucket.KeyAsString);
                    data.Add(bucket.DocCount);
                }

                var result = new
                {
                    labels,
                    data
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


    }
}