# Trình sửa ánh sáng — đèn tĩnh của cảnh, đặt trên bản xem trước ban đêm

Cảnh ban đêm của `time_base` được chiếu sáng bằng dòng `light_set` của kịch bản. Trình sửa ánh sáng biến dòng đó thành các điểm bạn kéo trên phông nền của cảnh: bản xem trước được vẽ bằng chính shader ban đêm của trò chơi, nên bạn thấy cảnh sẽ trông thế nào khi trời tối.

## Cách mở

Nút búa trên thanh công cụ trong trò chơi → **Trình sửa ánh sáng**.

## Đặt đèn

![Trình sửa ánh sáng với home_kitchen_cooking.scenario: hai đèn trong danh sách, phông nền nhà bếp trong bản xem trước ban đêm với ánh đèn và vòng bán kính của nó, đèn đã chọn có X, Y, Bán kính 500, Cường độ 1.0 và màu vàng.](assets/light-editor.jpg)

1. Chọn kịch bản có dòng `light_set` bạn sửa (● đánh dấu kịch bản đã đặt đèn). Phông nền của cảnh đó được hiển thị; **Phông nền xem trước** chọn phông khác.
2. Nhấp đúp lên nền (hoặc **+ Đèn**) để thêm đèn, kéo để di chuyển, Del xóa đèn đang chọn. Tối đa 10 đèn tĩnh.
3. **Đèn đã chọn**: **X**/**Y** (0–1 của màn hình), **Bán kính** (điểm ảnh của màn hình game rộng 1920 px), **Cường độ** (0–1) và **Màu**.
4. Ctrl+S ghi dòng `light_set` vào kịch bản; kịch bản chưa có dòng này sẽ được chèn sau phần đầu. **Sao chép dòng** thay vào đó đưa lệnh vào bộ nhớ tạm để dán tay.

## Giới hạn

- Chỉ mod dạng thư mục; không ghi được kịch bản đóng gói (vẫn sao chép được dòng).
- Đèn đi theo kịch bản: một kịch bản khác của cùng cảnh có thể đặt đèn khác.

## Phụ thuộc

`tools_base`, `time_base`, `ui_base`, `scenario_tools_base` (đọc kịch bản; không cần cửa sổ trình sửa kịch bản).
Gói cũ `editor_tools` cài mod này cùng các cửa sổ tác giả khác.
