// /wwwroot/js/product-slider-helper.js
var ProductSliderHelper = {
    // Khởi tạo slider với cách xử lý hình ảnh
    initializeSlider: function (sliderSelector, options) {
        var slider = $(sliderSelector);

        if (!slider.length) return;

        // Hủy slider nếu đã được khởi tạo 
        if (slider.hasClass('slick-initialized')) {
            slider.slick('unslick');
        }

        // Mặc định options nếu không được cung cấp
        var defaultOptions = {
            slidesToShow: 4,
            slidesToScroll: 1,
            arrows: true,
            dots: false,
            infinite: false,
            responsive: [
                {
                    breakpoint: 1200,
                    settings: {
                        slidesToShow: 3
                    }
                },
                {
                    breakpoint: 992,
                    settings: {
                        slidesToShow: 2
                    }
                },
                {
                    breakpoint: 576,
                    settings: {
                        slidesToShow: 1
                    }
                }
            ]
        };

        var slickOptions = $.extend(defaultOptions, options || {});

        // Đếm số lượng hình ảnh cần tải
        var images = slider.find('img');
        var totalImages = images.length;
        var loadedImages = 0;

        // Khởi tạo ngay nếu không có hình ảnh
        if (totalImages === 0) {
            slider.slick(slickOptions);
            return;
        }

        // Đặt kích thước cố định cho container trước khi tải hình ảnh
        var containerHeight = slider.height();
        if (containerHeight < 200) {
            slider.css('min-height', '300px');
        }

        // Khởi tạo trạng thái tải
        var loadingPlaceholder = $('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        slider.prepend(loadingPlaceholder);

        // Theo dõi quá trình tải hình ảnh
        images.each(function () {
            if (this.complete) {
                loadedImages++;
                if (loadedImages === totalImages) {
                    initSlider();
                }
            } else {
                $(this).on('load', function () {
                    loadedImages++;
                    if (loadedImages === totalImages) {
                        initSlider();
                    }
                });

                $(this).on('error', function () {
                    loadedImages++;
                    if (loadedImages === totalImages) {
                        initSlider();
                    }
                });
            }
        });

        // Đảm bảo slider vẫn được khởi tạo sau timeout
        setTimeout(function () {
            if (!slider.hasClass('slick-initialized')) {
                initSlider();
            }
        }, 2000);

        function initSlider() {
            loadingPlaceholder.remove();
            slider.slick(slickOptions);
        }
    }
};
