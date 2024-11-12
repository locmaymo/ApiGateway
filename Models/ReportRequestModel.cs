using System;
using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Models
{
    public class ReportRequestModel
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

    }
}