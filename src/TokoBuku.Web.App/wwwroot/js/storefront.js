// Storefront JavaScript Handler

document.addEventListener('DOMContentLoaded', function () {
    // 1. Initial cart badge update
    refreshCartBadge();

    // 2. Delegate Add to Cart Click
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-add-to-cart');
        if (btn) {
            e.preventDefault();
            const bookId = parseInt(btn.dataset.bookId);
            const qty = parseInt(btn.dataset.qty || 1);
            addToCartAjax(bookId, qty);
        }
    });
});

async function addToCartAjax(bookId, quantity) {
    try {
        const response = await fetch('/Cart/AddToCartAjax', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ bookId: bookId, quantity: quantity })
        });

        const data = await response.json();

        if (data.success) {
            updateBadge(data.totalItems);
            
            // Display user-friendly toast notification
            if (window.Swal) {
                Swal.fire({
                    toast: true,
                    position: 'bottom-end',
                    icon: 'success',
                    title: data.message,
                    showConfirmButton: false,
                    timer: 2500,
                    timerProgressBar: true
                });
            }

            // Open Cart Drawer
            const drawerEl = document.getElementById('cartDrawer');
            if (drawerEl && window.bootstrap) {
                const bsOffcanvas = bootstrap.Offcanvas.getOrCreateInstance(drawerEl);
                bsOffcanvas.show();
                loadCartDrawerContent();
            }
        } else {
            if (window.Swal) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Pemberitahuan',
                    text: data.message
                });
            } else {
                alert(data.message);
            }
        }
    } catch (err) {
        console.error('Add to cart failed:', err);
    }
}

async function refreshCartBadge() {
    try {
        const response = await fetch('/Cart/GetSummaryAjax');
        const data = await response.json();
        updateBadge(data.totalItems);
    } catch (err) {
        console.error('Failed to get cart summary:', err);
    }
}

function updateBadge(totalItems) {
    const badges = document.querySelectorAll('.cart-badge');
    badges.forEach(b => {
        b.textContent = totalItems;
        if (totalItems > 0) {
            b.classList.remove('d-none');
        } else {
            b.classList.add('d-none');
        }
    });
}

async function loadCartDrawerContent() {
    const container = document.getElementById('cartDrawerItemsContainer');
    const totalEl = document.getElementById('cartDrawerTotalAmount');
    if (!container) return;

    try {
        const response = await fetch('/Cart/GetSummaryAjax');
        const data = await response.json();

        if (data.items.length === 0) {
            container.innerHTML = `
                <div class="text-center py-5 text-muted">
                    <i class="bi bi-bag-x fs-1 d-block mb-2"></i>
                    Keranjang Anda masih kosong.
                </div>
            `;
            if (totalEl) totalEl.textContent = 'Rp 0';
            return;
        }

        let html = '';
        data.items.forEach(item => {
            const finalPriceFormatted = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', maximumFractionDigits: 0 }).format(item.finalPrice);
            const coverSrc = item.coverImageUrl ? (item.coverImageUrl.startsWith('/') || item.coverImageUrl.startsWith('http') ? item.coverImageUrl : `/images/books/${item.coverImageUrl}`) : '';
            html += `
                <div class="cart-drawer-item">
                    <div class="cart-drawer-img d-flex align-items-center justify-content-center text-muted">
                        ${coverSrc ? `<img src="${coverSrc}" alt="${item.title}" class="w-100 h-100 object-fit-cover rounded" />` : '<i class="bi bi-book fs-3"></i>'}
                    </div>
                    <div class="flex-grow-1 overflow-hidden">
                        <h6 class="mb-1 text-truncate" style="font-size: 0.9rem;">${item.title}</h6>
                        <div class="text-muted small">${item.author}</div>
                        <div class="fw-bold text-teal mt-1" style="color: #0f766e;">${finalPriceFormatted} <span class="text-muted fw-normal small">x ${item.quantity}</span></div>
                    </div>
                    <button class="btn btn-sm btn-link text-danger p-0 align-self-start" onclick="removeCartItemAjax(${item.bookId})" title="Hapus">
                        <i class="bi bi-trash3"></i>
                    </button>
                </div>
            `;
        });

        container.innerHTML = html;
        if (totalEl) {
            totalEl.textContent = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', maximumFractionDigits: 0 }).format(data.totalNetAmount);
        }
    } catch (err) {
        console.error('Error loading cart drawer:', err);
    }
}

async function removeCartItemAjax(bookId) {
    try {
        const response = await fetch('/Cart/RemoveItemAjax', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ bookId: bookId })
        });
        const data = await response.json();
        updateBadge(data.totalItems);
        loadCartDrawerContent();
    } catch (err) {
        console.error('Error removing item:', err);
    }
}
