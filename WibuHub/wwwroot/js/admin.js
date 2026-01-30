// Admin Dashboard JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // Sidebar toggle for mobile
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });

        // Close sidebar when clicking outside on mobile
        document.addEventListener('click', function (event) {
            const isClickInside = sidebar.contains(event.target) || sidebarToggle.contains(event.target);
            
            if (!isClickInside && window.innerWidth <= 768) {
                sidebar.classList.remove('show');
            }
        });
    }

    // Active navigation highlighting
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.sidebar-nav .nav-link');
    
    navLinks.forEach(link => {
        link.classList.remove('active');
        
        const linkPath = link.getAttribute('href');
        if (linkPath && (linkPath === currentPath || (currentPath.includes('/Admin/Dashboard') && linkPath.includes('Dashboard')))) {
            link.classList.add('active');
        }
    });

    // Add loading state to buttons
    const buttons = document.querySelectorAll('button[type="submit"], .btn-submit');
    buttons.forEach(button => {
        button.addEventListener('click', function (e) {
            if (this.form && !this.form.checkValidity()) {
                return;
            }
            
            const originalText = this.innerHTML;
            this.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Loading...';
            this.disabled = true;
            
            // Re-enable after 5 seconds as a fallback
            setTimeout(() => {
                this.innerHTML = originalText;
                this.disabled = false;
            }, 5000);
        });
    });
});
