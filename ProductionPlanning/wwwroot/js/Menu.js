document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("sidebar");
    const toggleBtnMobile = document.getElementById("toggleSidebar");
    const toggleBtnDesktop = document.getElementById("toggleSidebarDesktop");
    const menuItems = document.querySelectorAll(".menu-item");
    const root = document.documentElement;

    if (!sidebar) return;

    // 🔹 Toggle desktop
    if (toggleBtnDesktop) {
        toggleBtnDesktop.addEventListener("click", function (e) {
            e.stopPropagation();
            root.classList.toggle("sidebar-collapsed");
            localStorage.setItem("sidebar-collapsed", root.classList.contains("sidebar-collapsed"));
        });
    }

    // 🔹 Toggle mobile
    if (toggleBtnMobile) {
        toggleBtnMobile.addEventListener("click", function (e) {
            e.stopPropagation();
            root.classList.toggle("sidebar-mobile-hidden");
            localStorage.setItem("sidebar-mobile-hidden", root.classList.contains("sidebar-mobile-hidden"));
        });
    }

    // 🔹 Highlight active menu
    const currentPath = window.location.pathname.toLowerCase();
    menuItems.forEach(item => {
        const href = item.getAttribute("href");
        if (href && (currentPath === href.toLowerCase() || currentPath.startsWith(href.toLowerCase() + "/"))) {
            item.classList.add("active");
        }
    });

    // 🔹 Close sidebar on outside click (mobile)
    document.addEventListener("click", function (e) {
        if (window.innerWidth <= 768 &&
            !sidebar.contains(e.target) &&
            !(toggleBtnMobile && toggleBtnMobile.contains(e.target))) {
            root.classList.add("sidebar-mobile-hidden");
            localStorage.setItem("sidebar-mobile-hidden", "true");
        }
    });

    function handleSidebarResize() {
        const root = document.documentElement;
        if (window.innerWidth > 768) {
            // Если на большом экране, убрать mobile-hidden
            if (root.classList.contains("sidebar-mobile-hidden")) {
                root.classList.remove("sidebar-mobile-hidden");
            }
        } else {
            // На маленьком экране — оставляем текущее состояние из localStorage
            const isMobileHidden = localStorage.getItem("sidebar-mobile-hidden") === "true";
            if (isMobileHidden) {
                root.classList.add("sidebar-mobile-hidden");
            }
        }
    }

    // Выполнить при загрузке
    handleSidebarResize();

    // Отслеживаем ресайз
    window.addEventListener("resize", handleSidebarResize);

});
