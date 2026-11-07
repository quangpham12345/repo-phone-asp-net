# Ảnh thật của sản phẩm

Đặt ảnh vào thư mục con mang **đúng slug** của sản phẩm, rồi chạy lại web (`dotnet run`).
Lúc khởi động, `DbInitializer.DongBoAnhThat` tự gắn ảnh vào sản phẩm đang dùng ảnh minh hoạ.
Ảnh admin đã tải lên qua trang quản trị không bị đè.

```
wwwroot/images/sanpham/iphone-17-pro-max/
    chinh.jpg          ảnh đại diện (không có file này thì lấy file đầu tiên theo tên)
    cam-vu-tru.jpg     ảnh của biến thể màu "Cam Vũ Trụ" (tên file = slug của màu)
    xanh-dam.jpg       ảnh của màu "Xanh Đậm"
    2.jpg, 3.jpg       ảnh phụ thêm (gallery)
```

Nhận `.jpg` `.jpeg` `.png` `.webp` `.avif`. Nên dùng ảnh vuông, nền trắng, khoảng 800x800, dưới 300KB.

Ảnh banner trang chủ đặt ở `wwwroot/images/banners/` tên `1.jpg`, `2.jpg`, `3.jpg`, `4.jpg` (tỉ lệ khoảng 3:1, ví dụ 1600x520).

> Ảnh sản phẩm của hãng (Apple, Samsung...) có bản quyền. Dùng cho bài tập trên lớp thì nên ghi nguồn
> trong báo cáo; không dùng cho mục đích thương mại.

## Danh sách thư mục và tên file theo màu

| Danh mục | Sản phẩm | Thư mục | File ảnh theo màu |
|---|---|---|---|
| Điện thoại | iPhone 17 Pro Max | `iphone-17-pro-max` | `cam-vu-tru.jpg`, `xanh-dam.jpg`, `bac.jpg` |
| Điện thoại | iPhone 17 Pro | `iphone-17-pro` | `cam-vu-tru.jpg`, `xanh-dam.jpg`, `bac.jpg` |
| Điện thoại | iPhone Air | `iphone-air` | `den-khong-gian.jpg`, `trang-may.jpg`, `vang-nhat.jpg`, `xanh-da-troi.jpg` |
| Điện thoại | iPhone 17 | `iphone-17` | `den.jpg`, `trang.jpg`, `xanh-suong-mu.jpg`, `tim-oai-huong.jpg`, `xanh-la-xo-thom.jpg` |
| Điện thoại | iPhone 16 | `iphone-16` | `den.jpg`, `trang.jpg`, `xanh-mong-ket.jpg`, `hong.jpg` |
| Điện thoại | iPhone 16e | `iphone-16e` | `den.jpg`, `trang.jpg` |
| Điện thoại | iPhone 15 | `iphone-15` | `den.jpg`, `hong.jpg`, `xanh-duong.jpg` |
| Điện thoại | Samsung Galaxy S26 Ultra | `samsung-galaxy-s26-ultra` | `den-titan.jpg`, `xam-titan.jpg`, `xanh-titan.jpg` |
| Điện thoại | Samsung Galaxy S26 Plus | `samsung-galaxy-s26-plus` | `den.jpg`, `bac.jpg`, `xanh-navy.jpg` |
| Điện thoại | Samsung Galaxy S26 | `samsung-galaxy-s26` | `den.jpg`, `bac.jpg`, `xanh-navy.jpg`, `tim.jpg` |
| Điện thoại | Samsung Galaxy Z Fold7 | `samsung-galaxy-z-fold7` | `xanh-navy.jpg`, `bac.jpg`, `den.jpg` |
| Điện thoại | Samsung Galaxy Z Flip7 | `samsung-galaxy-z-flip7` | `den.jpg`, `xanh-navy.jpg`, `do-san-ho.jpg` |
| Điện thoại | Samsung Galaxy S25 FE | `samsung-galaxy-s25-fe` | `xanh-navy.jpg`, `trang.jpg`, `den.jpg` |
| Điện thoại | Samsung Galaxy A56 5G | `samsung-galaxy-a56-5g` | `den.jpg`, `hong.jpg`, `xam.jpg` |
| Điện thoại | Samsung Galaxy A17 5G | `samsung-galaxy-a17-5g` | `den.jpg`, `xanh.jpg`, `xam.jpg` |
| Điện thoại | Xiaomi 15T Pro | `xiaomi-15t-pro` | `den.jpg`, `xam.jpg`, `vang-mocha.jpg` |
| Điện thoại | Xiaomi 15T | `xiaomi-15t` | `den.jpg`, `xam.jpg`, `hong.jpg` |
| Điện thoại | Xiaomi Redmi Note 14 Pro+ 5G | `xiaomi-redmi-note-14-pro-5g` | `den.jpg`, `tim.jpg`, `xanh-bang.jpg` |
| Điện thoại | Xiaomi Redmi Note 14 | `xiaomi-redmi-note-14` | `den.jpg`, `xanh-la.jpg`, `tim.jpg` |
| Điện thoại | OPPO Find X8 | `oppo-find-x8` | `den-vu-tru.jpg`, `trang-ngoc-trai.jpg` |
| Điện thoại | OPPO Reno14 5G | `oppo-reno14-5g` | `trang-ngoc-trai.jpg`, `xanh-ngoc.jpg`, `xam.jpg` |
| Điện thoại | OPPO Reno14 F 5G | `oppo-reno14-f-5g` | `xanh-la.jpg`, `hong.jpg`, `xanh-duong.jpg` |
| Điện thoại | OPPO A5 Pro 5G | `oppo-a5-pro-5g` | `den.jpg`, `nau.jpg` |
| Điện thoại | vivo V50 | `vivo-v50` | `xam-titan.jpg`, `hong.jpg` |
| Điện thoại | vivo Y29 | `vivo-y29` | `den.jpg`, `trang.jpg` |
| Tablet | iPad Pro M5 11 inch WiFi | `ipad-pro-m5-11-inch-wifi` | `den-khong-gian.jpg`, `bac.jpg` |
| Tablet | iPad Air M3 11 inch WiFi | `ipad-air-m3-11-inch-wifi` | `xam-khong-gian.jpg`, `xanh-duong.jpg`, `tim.jpg`, `anh-sao.jpg` |
| Tablet | iPad 11 WiFi | `ipad-11-wifi` | `xanh-duong.jpg`, `hong.jpg`, `vang.jpg`, `bac.jpg` |
| Tablet | iPad mini 7 WiFi | `ipad-mini-7-wifi` | `xam-khong-gian.jpg`, `xanh-duong.jpg`, `tim.jpg` |
| Tablet | Samsung Galaxy Tab S11 | `samsung-galaxy-tab-s11` | `xam.jpg`, `bac.jpg` |
| Tablet | Samsung Galaxy Tab S10 FE | `samsung-galaxy-tab-s10-fe` | `xam.jpg`, `bac.jpg`, `xanh-duong.jpg` |
| Tablet | Xiaomi Pad 7 | `xiaomi-pad-7` | `xam.jpg`, `xanh.jpg` |
| Tablet | Xiaomi Redmi Pad 2 | `xiaomi-redmi-pad-2` | `xam.jpg`, `xanh-la.jpg` |
| Laptop | MacBook Air M4 13 inch | `macbook-air-m4-13-inch` | `xanh-da-troi.jpg`, `bac.jpg`, `anh-sao.jpg`, `den-xam.jpg` |
| Laptop | MacBook Air M4 15 inch | `macbook-air-m4-15-inch` | `xanh-da-troi.jpg`, `bac.jpg`, `den-xam.jpg` |
| Laptop | MacBook Pro 14 M5 | `macbook-pro-14-m5` | `den-khong-gian.jpg`, `bac.jpg` |
| Laptop | MacBook Pro 14 M4 Pro | `macbook-pro-14-m4-pro` | `den-khong-gian.jpg` |
| Laptop | ASUS Vivobook 15 OLED | `asus-vivobook-15-oled` | `bac.jpg`, `xanh-den.jpg` |
| Laptop | ASUS Zenbook 14 OLED | `asus-zenbook-14-oled` | `xanh-den.jpg`, `trang-su.jpg` |
| Laptop | ASUS ROG Strix G16 | `asus-rog-strix-g16` | `xam-eclipse.jpg` |
| Laptop | Lenovo IdeaPad Slim 5 | `lenovo-ideapad-slim-5` | `xam.jpg`, `xanh.jpg` |
| Laptop | Lenovo Legion 5 Gen 10 | `lenovo-legion-5-gen-10` | `xam-lunar.jpg` |
| Laptop | Lenovo ThinkPad E14 Gen 6 | `lenovo-thinkpad-e14-gen-6` | `den.jpg` |
| Laptop | Dell Inspiron 14 5440 | `dell-inspiron-14-5440` | `bac.jpg`, `xanh-den.jpg` |
| Phụ kiện | AirPods Pro 3 | `airpods-pro-3` | `trang.jpg` |
| Phụ kiện | AirPods 4 chống ồn chủ động | `airpods-4-chong-on-chu-dong` | `trang.jpg` |
| Phụ kiện | Apple Watch Series 11 GPS 42mm | `apple-watch-series-11-gps-42mm` | `den-bong.jpg`, `bac.jpg`, `vang-hong.jpg` |
| Phụ kiện | Ốp lưng iPhone 17 Pro Clear Case MagSafe | `op-lung-iphone-17-pro-clear-case-magsafe` | `trong-suot.jpg` |
| Phụ kiện | Cáp sạc Apple USB-C 1m | `cap-sac-apple-usb-c-1m` | `trang.jpg` |
| Phụ kiện | Samsung Galaxy Buds3 Pro | `samsung-galaxy-buds3-pro` | `bac.jpg`, `trang.jpg` |
| Phụ kiện | Samsung Galaxy Watch8 44mm | `samsung-galaxy-watch8-44mm` | `bac.jpg`, `den.jpg` |
| Phụ kiện | Sony WH-1000XM6 | `sony-wh-1000xm6` | `den.jpg`, `bac.jpg`, `xanh-dem.jpg` |
| Phụ kiện | Sony WF-1000XM5 | `sony-wf-1000xm5` | `den.jpg`, `bac.jpg` |
| Phụ kiện | Sạc nhanh Anker Nano 65W 3 cổng | `sac-nhanh-anker-nano-65w-3-cong` | `trang.jpg`, `den.jpg` |
| Phụ kiện | Pin dự phòng Anker 20000mAh 30W | `pin-du-phong-anker-20000mah-30w` | `den.jpg`, `trang.jpg` |
| Phụ kiện | Củ sạc Xiaomi 67W kèm cáp | `cu-sac-xiaomi-67w-kem-cap` | `trang.jpg` |
