/* Essenz Store – site.js */
(function () {
    'use strict';

    // ===================== HEADER SCROLL =====================
    const header = document.getElementById('header');
    if (header) {
        window.addEventListener('scroll', () => {
            header.classList.toggle('scrolled', window.scrollY > 30);
        }, { passive: true });
    }

    // ===================== HAMBURGER MENU =====================
    const hamburger = document.getElementById('hamburger');
    const mainNav = document.getElementById('mainNav');
    const mobileOverlay = document.getElementById('mobileOverlay');

    if (hamburger && mainNav) {
        hamburger.addEventListener('click', () => {
            const open = mainNav.classList.toggle('open');
            document.body.style.overflow = open ? 'hidden' : '';
            if (mobileOverlay) mobileOverlay.style.display = open ? 'block' : 'none';
        });
        if (mobileOverlay) {
            mobileOverlay.addEventListener('click', () => {
                mainNav.classList.remove('open');
                document.body.style.overflow = '';
                mobileOverlay.style.display = 'none';
            });
        }
    }

    // ===================== SEARCH TOGGLE =====================
    const searchToggle = document.getElementById('searchToggle');
    const searchBar = document.getElementById('searchBar');
    const searchClose = document.getElementById('searchClose');

    if (searchToggle && searchBar) {
        searchToggle.addEventListener('click', () => {
            searchBar.classList.toggle('open');
            if (searchBar.classList.contains('open')) searchBar.querySelector('input')?.focus();
        });
        if (searchClose) searchClose.addEventListener('click', () => searchBar.classList.remove('open'));
        document.addEventListener('keydown', e => { if (e.key === 'Escape') searchBar.classList.remove('open'); });
    }

    // ===================== FILTER SIDEBAR (mobile) =====================
    const filterToggle = document.getElementById('filterToggle');
    const filterSidebar = document.getElementById('filterSidebar');
    const filterClose = document.getElementById('filterClose');
    const filterOverlay = document.getElementById('filterOverlay');

    if (filterToggle && filterSidebar) {
        filterToggle.addEventListener('click', () => {
            filterSidebar.classList.add('open');
            if (filterOverlay) filterOverlay.classList.add('open');
            document.body.style.overflow = 'hidden';
        });
        const closeFilter = () => {
            filterSidebar.classList.remove('open');
            if (filterOverlay) filterOverlay.classList.remove('open');
            document.body.style.overflow = '';
        };
        if (filterClose) filterClose.addEventListener('click', closeFilter);
        if (filterOverlay) filterOverlay.addEventListener('click', closeFilter);
    }

    // ===================== HERO SLIDER =====================
    const heroSlider = document.getElementById('heroSlider');
    if (heroSlider) {
        const slides = heroSlider.querySelectorAll('.hero__slide');
        const dotsContainer = document.getElementById('heroDots');
        const prev = document.getElementById('heroPrev');
        const next = document.getElementById('heroNext');
        let current = 0, autoplay;

        if (slides.length > 1) {
            // Create dots
            slides.forEach((_, i) => {
                const dot = document.createElement('button');
                dot.className = 'hero__dot' + (i === 0 ? ' active' : '');
                dot.setAttribute('aria-label', 'Slide ' + (i + 1));
                dot.addEventListener('click', () => goTo(i));
                if (dotsContainer) dotsContainer.appendChild(dot);
            });

            const goTo = (idx) => {
                slides[current].style.display = 'none';
                current = (idx + slides.length) % slides.length;
                slides[current].style.display = 'flex';
                dotsContainer?.querySelectorAll('.hero__dot').forEach((d, i) => d.classList.toggle('active', i === current));
            };

            // Init: hide all but first
            slides.forEach((s, i) => { s.style.display = i === 0 ? 'flex' : 'none'; });

            prev?.addEventListener('click', () => { clearInterval(autoplay); goTo(current - 1); startAuto(); });
            next?.addEventListener('click', () => { clearInterval(autoplay); goTo(current + 1); startAuto(); });

            const startAuto = () => { autoplay = setInterval(() => goTo(current + 1), 5000); };
            startAuto();
        }
    }

    // ===================== MINI CART =====================
    const miniCartToggle = document.getElementById('miniCartToggle');
    const miniCart = document.getElementById('miniCart');
    const miniCartClose = document.getElementById('miniCartClose');
    const miniCartOverlay = document.getElementById('miniCartOverlay');

    function openMiniCart() {
        miniCart?.classList.add('open');
        miniCartOverlay?.classList.add('open');
        document.body.style.overflow = 'hidden';
        loadMiniCart();
    }
    function closeMiniCart() {
        miniCart?.classList.remove('open');
        miniCartOverlay?.classList.remove('open');
        document.body.style.overflow = '';
    }
    miniCartToggle?.addEventListener('click', openMiniCart);
    miniCartClose?.addEventListener('click', closeMiniCart);
    miniCartOverlay?.addEventListener('click', closeMiniCart);

    async function loadMiniCart() {
        try {
            const r = await fetch('/Cart/MiniCart');
            if (!r.ok) return;
            const data = await r.json();
            renderMiniCart(data);
        } catch (_) {}
    }

    function renderMiniCart(data) {
        const itemsEl = document.getElementById('miniCartItems');
        const footerEl = document.getElementById('miniCartFooter');
        const countEl = document.getElementById('miniCartCount');
        const totalEl = document.getElementById('miniCartTotal');
        const installEl = document.getElementById('miniCartInstallment');
        const freteMsg = document.getElementById('freteMsg');
        const freteBar = document.getElementById('freteProgressFill');
        const cartCountEl = document.getElementById('cartCount');

        const items = data.items || [];
        const total = data.total || 0;
        const count = data.count || 0;

        if (countEl) countEl.textContent = '(' + count + ')';
        if (cartCountEl) cartCountEl.textContent = count;

        // Frete progress
        const threshold = window.FRETE_GRATIS || 299;
        const pct = Math.min((total / threshold) * 100, 100);
        if (freteBar) freteBar.style.width = pct + '%';
        if (freteMsg) {
            if (total >= threshold) {
                freteMsg.textContent = '🎉 Parabéns! Você ganhou frete grátis!';
                freteMsg.className = 'frete-msg achieved';
            } else {
                const diff = (threshold - total).toFixed(2).replace('.', ',');
                freteMsg.textContent = `Faltam R$ ${diff} para frete grátis`;
                freteMsg.className = 'frete-msg';
            }
        }

        if (items.length === 0) {
            if (itemsEl) itemsEl.innerHTML = `<div class="mini-cart__empty"><i class="fas fa-shopping-bag"></i><p>Seu carrinho está vazio</p><a href="/Products/Index" class="btn btn--primary btn--sm">Ver produtos</a></div>`;
            if (footerEl) footerEl.style.display = 'none';
            return;
        }

        if (itemsEl) {
            itemsEl.innerHTML = items.map(item => `
                <div class="mini-cart-item">
                    <img src="${item.imageUrl || '/img/placeholder-product.jpg'}" class="mini-cart-item__img" alt="${item.nome}" />
                    <div class="mini-cart-item__info">
                        <span class="mini-cart-item__name">${item.nome}</span>
                        ${item.variante ? `<span class="mini-cart-item__var">${item.variante}</span>` : ''}
                        <span class="mini-cart-item__price">R$ ${item.precoUnit.toFixed(2).replace('.', ',')} x${item.quantidade}</span>
                    </div>
                    <button class="mini-cart-item__remove" onclick="removeFromMiniCart(${item.cartItemId})" title="Remover"><i class="fas fa-trash-alt"></i></button>
                </div>`).join('');
        }

        if (totalEl) totalEl.textContent = 'R$ ' + total.toFixed(2).replace('.', ',');
        if (installEl) installEl.textContent = `ou 6x de R$ ${(total / 6).toFixed(2).replace('.', ',')} sem juros`;
        if (footerEl) footerEl.style.display = 'block';
    }

    window.removeFromMiniCart = async function(cartItemId) {
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const form = new FormData();
            form.append('id', cartItemId);
            const headers = token ? { 'RequestVerificationToken': token } : {};
            await fetch('/Cart/Remove', { method: 'POST', body: form, headers });
            loadMiniCart();
        } catch (_) {}
    };

    // ===================== CART COUNT =====================
    async function updateCartCount() {
        try {
            const r = await fetch('/Cart/Count');
            if (r.ok) {
                const data = await r.json();
                const el = document.getElementById('cartCount');
                if (el) el.textContent = data.count || 0;
            }
        } catch (_) {}
    }
    updateCartCount();

    // ===================== ADD TO CART (product card) =====================
    async function addToCart(productId, variantId = 0, qty = 1) {
        try {
            const form = new FormData();
            form.append('ProductId', productId);
            form.append('VariantId', variantId);
            form.append('Quantidade', qty);
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const headers = token ? { 'RequestVerificationToken': token } : {};
            const r = await fetch('/Cart/Add', { method: 'POST', body: form, headers });
            if (r.ok || r.redirected) {
                showToast('Produto adicionado ao carrinho! 🛒', 'success');
                updateCartCount();
                openMiniCart();
            }
        } catch (_) { showToast('Erro ao adicionar produto.', 'error'); }
    }

    document.querySelectorAll('.product-card__quick-add').forEach(btn => {
        btn.addEventListener('click', (e) => { e.preventDefault(); addToCart(btn.dataset.productId); });
    });

    document.querySelectorAll('.product-card__buy[data-product-id]').forEach(btn => {
        btn.addEventListener('click', (e) => { e.preventDefault(); addToCart(btn.dataset.productId); });
    });

    window.quickAddSize = function(e, productId, tamanho) {
        e.preventDefault(); e.stopPropagation();
        addToCart(productId, 0, 1);
    };

    // ===================== NEWSLETTER =====================
    const nlForm = document.getElementById('newsletterForm');
    if (nlForm) {
        nlForm.addEventListener('submit', (e) => {
            e.preventDefault();
            showToast('Cadastro realizado! Obrigado. 🎉', 'success');
            nlForm.reset();
        });
    }

    // ===================== TOAST =====================
    function showToast(msg, type = 'success') {
        const toast = document.createElement('div');
        toast.className = 'alert alert--' + type;
        toast.innerHTML = `<i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>${msg}`;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 3500);
    }
    window.showToast = showToast;

    // ===================== ALERT AUTO-DISMISS =====================
    document.querySelectorAll('.alert').forEach(alert => {
        setTimeout(() => alert.remove(), 4000);
    });

    // ===================== PAYMENT METHOD TOGGLE =====================
    document.querySelectorAll('.payment-method input[type=radio]').forEach(radio => {
        radio.addEventListener('change', () => {
            document.querySelectorAll('.payment-method').forEach(l => l.classList.remove('active'));
            radio.closest('.payment-method').classList.add('active');
        });
    });

    // ===================== CEP MASK =====================
    document.querySelectorAll('[placeholder*="CEP"], #cepField, #cepInput').forEach(inp => {
        inp.addEventListener('input', (e) => {
            let val = e.target.value.replace(/\D/g, '').slice(0, 8);
            if (val.length > 5) val = val.slice(0, 5) + '-' + val.slice(5);
            e.target.value = val;
        });
    });

    // ===================== CPF MASK =====================
    document.querySelectorAll('[placeholder*="CPF"], [id*="Cpf"]').forEach(inp => {
        inp.addEventListener('input', (e) => {
            let v = e.target.value.replace(/\D/g, '').slice(0, 11);
            if (v.length > 9) v = v.slice(0, 3) + '.' + v.slice(3, 6) + '.' + v.slice(6, 9) + '-' + v.slice(9);
            else if (v.length > 6) v = v.slice(0, 3) + '.' + v.slice(3, 6) + '.' + v.slice(6);
            else if (v.length > 3) v = v.slice(0, 3) + '.' + v.slice(3);
            e.target.value = v;
        });
    });

    // ===================== CARD NUMBER MASK =====================
    const cardInput = document.querySelector('[name="CartaoNumero"]');
    if (cardInput) {
        cardInput.addEventListener('input', (e) => {
            let v = e.target.value.replace(/\D/g, '').slice(0, 16);
            e.target.value = v.replace(/(.{4})/g, '$1 ').trim();
        });
    }

    // ===================== CARD EXPIRY MASK =====================
    const expiryInput = document.querySelector('[name="CartaoValidade"]');
    if (expiryInput) {
        expiryInput.addEventListener('input', (e) => {
            let v = e.target.value.replace(/\D/g, '').slice(0, 4);
            if (v.length > 2) v = v.slice(0, 2) + '/' + v.slice(2);
            e.target.value = v;
        });
    }

    // ===================== ANNOUNCEMENT BAR =====================
    const track = document.getElementById('announcementTrack');
    if (track) {
        track.innerHTML += track.innerHTML; // duplicate for seamless loop
    }

    // ===================== LGPD COOKIE BANNER =====================
    (function () {
        const banner = document.getElementById('lgpdBanner');
        if (!banner) return;
        if (!localStorage.getItem('lgpd_consent')) {
            setTimeout(() => { banner.style.display = 'flex'; }, 1500);
        }
        window.aceitarCookies = function () {
            localStorage.setItem('lgpd_consent', 'all');
            banner.style.animation = 'none';
            banner.style.transform = 'translateY(100%)';
            banner.style.opacity = '0';
            banner.style.transition = 'transform .4s ease, opacity .3s ease';
            setTimeout(() => { banner.style.display = 'none'; }, 400);
        };
        window.recusarCookies = function () {
            localStorage.setItem('lgpd_consent', 'essential');
            banner.style.transform = 'translateY(100%)';
            banner.style.transition = 'transform .4s ease';
            setTimeout(() => { banner.style.display = 'none'; }, 400);
        };
    })();

})();
