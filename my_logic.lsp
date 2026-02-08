;;; FILE: my_logic.lsp
;;; ĐỊNH NGHĨA LỆNH: HELLO_CONG

(defun C:HELLO_CONG (/ p1)
  ;; 1. In thông báo ra dòng Command
  (princ "\n--- ĐANG CHẠY LISP TỪ TRONG FILE DLL ---")
  
  ;; 2. Yêu cầu người dùng chọn một điểm
  (setq p1 (getpoint "\nChọn tâm để vẽ hình tròn: "))
  
  ;; 3. Nếu người dùng đã chọn điểm, tiến hành vẽ
  (if p1
    (progn
      ;; Vẽ hình tròn bán kính 100 tại điểm p1
      (command "._CIRCLE" p1 100)
      (princ "\n[Thành công] Đã vẽ hình tròn bán kính 100.")
    )
    (princ "\n[Lỗi] Bạn chưa chọn điểm.")
  )

  ;; Kết thúc gọn gàng
  (princ)
)

;; In thông báo khi file được load vào bộ nhớ
(princ "\n[Hệ thống] Mã nguồn Lisp đã được nạp vào bộ nhớ AutoCAD.")
(princ)
