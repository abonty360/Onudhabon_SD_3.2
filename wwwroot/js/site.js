// Site-wide JavaScript for Onudhabon

document.addEventListener('DOMContentLoaded', function () {
    // Dynamic Navbar Scroll Effect (turns white when scrolled down)
    const header = document.querySelector('.site-header');
    if (header) {
        function handleNavbarScroll() {
            if (window.scrollY > 20) {
                header.classList.add('scrolled');
            } else {
                header.classList.remove('scrolled');
            }
        }

        window.addEventListener('scroll', handleNavbarScroll, { passive: true });
        handleNavbarScroll(); // Initial check on page load / refresh
    }
});
