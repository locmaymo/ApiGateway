// khởi tạo select2 khi modal bật
$('#filterModal').on('shown.bs.modal', function () {
        // Kiểm tra nếu Select2 chưa được khởi tạo
        if (!$('#field-input').hasClass("select2-hidden-accessible")) {
    $('#field-input').select2({
        tags: true,
        placeholder: "Nhập hoặc chọn",
        allowClear: true,
        dropdownParent: $('#filterModal') // Gắn dropdown vào modal
    });
        }
});

$(document).ready(function () {
    // Biến để lưu trữ các bộ lọc
    var filters = [];
    var editingFilter = null; // Biến để lưu bộ lọc đang chỉnh sửa