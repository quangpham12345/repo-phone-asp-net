(function () {
    var input = document.getElementById('tuKhoaSearch');
    var box = document.getElementById('goiYBox');
    if (!input || !box) return;

    var timer = null;
    input.addEventListener('input', function () {
        clearTimeout(timer);
        var q = input.value.trim();
        if (q.length < 2) { box.style.display = 'none'; box.innerHTML = ''; return; }
        timer = setTimeout(function () {
            fetch('/Product/GoiY?q=' + encodeURIComponent(q))
                .then(function (r) { return r.json(); })
                .then(function (list) {
                    if (!list || !list.length) { box.style.display = 'none'; return; }
                    box.innerHTML = list.map(function (item) {
                        var img = item.imageUrl || '/images/products/no-image.svg';
                        var price = new Intl.NumberFormat('vi-VN').format(item.price) + 'đ';
                        return '<a class="list-group-item list-group-item-action d-flex align-items-center gap-2 py-2" href="/Product/Detail/' + item.slug + '">' +
                            '<img src="' + img + '" width="40" height="40" style="object-fit:cover" class="rounded" alt="">' +
                            '<div class="flex-grow-1"><div class="small fw-semibold">' + item.name + '</div>' +
                            '<div class="text-danger small">' + price + '</div></div></a>';
                    }).join('');
                    box.style.display = 'block';
                })
                .catch(function () { box.style.display = 'none'; });
        }, 250);
    });

    document.addEventListener('click', function (e) {
        if (!box.contains(e.target) && e.target !== input) box.style.display = 'none';
    });
})();
