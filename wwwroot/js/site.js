// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    // --- Chức năng Like (đã có từ trước) ---
    $(".like-button").each(function () {
        var button = $(this);
        var postId = button.data("post-id");
        if (postId) {
            $.ajax({
                url: '/api/Like/GetLikeCount/' + postId,
                type: 'GET',
                success: function (data) {
                    button.find(".like-count").text(data.likeCount);
                    if (data.liked) {
                        button.removeClass("btn-outline-primary").addClass("btn-primary");
                    } else {
                        button.removeClass("btn-primary").addClass("btn-outline-primary");
                    }
                },
                error: function (error) {
                    console.error("Error fetching like count: ", error);
                }
            });
        }
    });

    $(".like-button").on("click", function () {
        var button = $(this);
        var postId = button.data("post-id");
        var likeCountSpan = button.find(".like-count");

        if (postId) {
            $.ajax({
                url: '/api/Like/ToggleLike',
                type: 'POST',
                data: { postId: postId },
                headers: {
                    RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (data) {
                    likeCountSpan.text(data.likeCount);
                    if (data.liked) {
                        button.removeClass("btn-outline-primary").addClass("btn-primary");
                    } else {
                        button.removeClass("btn-primary").addClass("btn-outline-primary");
                    }
                },
                error: function (xhr, status, error) {
                    console.error("Error toggling like: ", xhr.responseText);
                    if (xhr.status === 401) {
                        alert("Please log in to like a post.");
                        window.location.href = "/Identity/Account/Login";
                    } else if (xhr.status === 404) {
                        alert("The post was not found.");
                    } else {
                        alert("An error occurred. Please try again.");
                    }
                }
            });
        }
    });

    // --- Chức năng Report (Mới) ---

    // Gán postId vào modal khi nút Report được click
    $(".report-button").on("click", function () {
        var postId = $(this).data("post-id");
        $("#reportPostId").val(postId);
        $("#reportReason").val(""); // Xóa nội dung cũ
        $("#reportReasonValidation").text(""); // Xóa lỗi validation cũ
        var reportModal = new bootstrap.Modal(document.getElementById('reportPostModal'));
        reportModal.show();
    });

    // Xử lý việc submit form trong modal Report
    $("#reportPostForm").on("submit", function (event) {
        event.preventDefault(); // Ngăn chặn submit form mặc định

        var form = $(this);
        var postId = $("#reportPostId").val();
        var reason = $("#reportReason").val();

        // Basic client-side validation
        if (!reason || reason.length < 5 || reason.length > 500) {
            $("#reportReasonValidation").text("Reason must be between 5 and 500 characters.");
            return; // Dừng nếu validation thất bại
        }

        $("#reportReasonValidation").text(""); // Xóa lỗi nếu có

        $.ajax({
            url: form.attr("action"), // Lấy URL từ thuộc tính action của form
            type: form.attr("method"), // Lấy method từ thuộc tính method của form
            data: form.serialize(), // Serialize tất cả dữ liệu form
            headers: {
                RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    alert("Report submitted successfully!");
                    var reportModal = bootstrap.Modal.getInstance(document.getElementById('reportPostModal'));
                    if (reportModal) reportModal.hide();
                } else {
                    alert("Failed to submit report: " + response.message);
                }
            },
            error: function (xhr, status, error) {
                console.error("Error submitting report: ", xhr.responseText);
                if (xhr.status === 401) {
                    alert("Please log in to report a post.");
                    window.location.href = "/Identity/Account/Login";
                } else if (xhr.responseJSON && xhr.responseJSON.message) {
                    alert("Error: " + xhr.responseJSON.message);
                } else {
                    alert("An error occurred while submitting the report.");
                }
            }
        });
    });

    // --- Thông báo (bell icon) ---
    const notificationDropdown = $('#notificationDropdown');
    const notificationList = $('#notificationList');
    const notificationBadge = $('#notificationBadge');

    function renderNotifications(data) {
        notificationList.empty();

        if (!data || !data.items || data.items.length === 0) {
            notificationList.append('<div class="p-3 text-center text-muted small">No notifications</div>');
            notificationBadge.addClass('d-none');
            return;
        }

        data.items.forEach(function (n) {
            const time = new Date(n.createdAt);
            const timeText = time.toLocaleString();

            const item = $('<a class="dropdown-item small py-2 px-3 d-flex justify-content-between align-items-start" href="' + (n.link || '#') + '"></a>');

            const textDiv = $('<div class="me-2"></div>');
            textDiv.append('<div>' + n.message + '</div>');
            textDiv.append('<div class="text-muted" style="font-size: 11px;">' + timeText + '</div>');

            item.append(textDiv);

            if (!n.isRead) {
                item.append('<span class="badge bg-primary rounded-pill ms-2" style="font-size: 9px;">New</span>');
            }

            notificationList.append(item);
        });

        if (data.unreadCount && data.unreadCount > 0) {
            notificationBadge.text(data.unreadCount);
            notificationBadge.removeClass('d-none');
        } else {
            notificationBadge.addClass('d-none');
        }
    }

    function loadNotifications() {
        $.ajax({
            url: '/api/Notifications/Recent',
            type: 'GET',
            success: function (data) {
                renderNotifications(data);
            },
            error: function (xhr) {
                console.error('Error loading notifications', xhr);
            }
        });
    }

    if (notificationDropdown.length) {
        // Load khi bấm vào chuông
        notificationDropdown.on('click', function () {
            loadNotifications();
        });

        // Nút mark all read
        $('#markAllNotificationsRead').on('click', function (e) {
            e.preventDefault();
            $.ajax({
                url: '/api/Notifications/MarkAllAsRead',
                type: 'POST',
                headers: {
                    RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                success: function () {
                    // Sau khi đánh dấu đã đọc, load lại để cập nhật
                    loadNotifications();
                },
                error: function (xhr) {
                    console.error('Error marking notifications as read', xhr);
                }
            });
        });

        // Thỉnh thoảng refresh badge (mỗi 60s)
        setInterval(function () {
            $.ajax({
                url: '/api/Notifications/Recent',
                type: 'GET',
                success: function (data) {
                    if (data && data.unreadCount && data.unreadCount > 0) {
                        notificationBadge.text(data.unreadCount);
                        notificationBadge.removeClass('d-none');
                    } else {
                        notificationBadge.addClass('d-none');
                    }
                }
            });
        }, 60000);
    }
});
