// site.js - All JavaScript functionality for the CMCS application

// Dark/Light Mode Toggle functionality
function initializeThemeToggle() {
    const themeToggle = document.getElementById("themeToggle");
    const body = document.body;

    if (!themeToggle) return;

    // Load saved theme on page load
    const savedTheme = localStorage.getItem("theme");
    if (savedTheme === "dark") {
        body.classList.add("dark-mode");
        themeToggle.innerHTML = '<i class="bi bi-sun"></i>';
    } else {
        themeToggle.innerHTML = '<i class="bi bi-moon"></i>';
    }

    // Toggle dark/light mode
    themeToggle.addEventListener("click", () => {
        body.classList.toggle("dark-mode");
        if (body.classList.contains("dark-mode")) {
            localStorage.setItem("theme", "dark");
            themeToggle.innerHTML = '<i class="bi bi-sun"></i>';
        } else {
            localStorage.setItem("theme", "light");
            themeToggle.innerHTML = '<i class="bi bi-moon"></i>';
        }
    });
}

// Dashboard search handling
function initializeSearch() {
    const dashboardSearchForm = document.getElementById("dashboardSearchForm");
    if (!dashboardSearchForm) return;

    dashboardSearchForm.addEventListener("submit", function (e) {
        e.preventDefault();
        const query = document.getElementById("dashboardSearch").value.trim().toLowerCase();
        if (!query) return; // do nothing if empty

        // Simple redirect - the application will handle authentication automatically
        if (query.includes("lecturer")) {
            window.location.href = '/Lecturer/Dashboard';
        }
        else if (query.includes("coordinator") || query.includes("programme coordinator")) {
            window.location.href = '/Coordinator/Dashboard';
        }
        else if (query.includes("manager") || query.includes("academic manager")) {
            window.location.href = '/Manager/Dashboard';
        }
        else if (query.includes("hr") || query.includes("human resources")) {
            window.location.href = '/HR/Dashboard';
        }
        else if (query.includes("login") || query.includes("sign in")) {
            window.location.href = '/Account/Login';
        }
        else if (query.includes("home")) {
            window.location.href = '/Home/Index';
        }
        else {
            // Show helpful error message
            alert('No matching dashboard found. Try searching for: "Lecturer", "Coordinator", "Manager", or "HR"');

            // Clear and focus the search field
            document.getElementById("dashboardSearch").value = '';
            document.getElementById("dashboardSearch").focus();
        }
    });
}

// Add helpful placeholder behavior for search
function initializeSearchPlaceholder() {
    const searchInput = document.getElementById('dashboardSearch');
    if (!searchInput) return;

    searchInput.addEventListener('focus', function () {
        this.placeholder = 'Search for: Lecturer, Coordinator, Manager, HR';
    });

    searchInput.addEventListener('blur', function () {
        this.placeholder = 'Try: Lecturer, Coordinator, Manager, HR';
    });
}

// Handle search errors from URL parameters
function handleSearchErrors() {
    const urlParams = new URLSearchParams(window.location.search);
    const searchError = urlParams.get('searchError');
    if (searchError) {
        console.log('Search error:', searchError);
        // You could show a toast notification here if desired
    }
}

// Auto-calculation for claim submission forms
function initializeClaimAutoCalculation() {
    const hoursWorkedInput = document.getElementById('hoursWorked');
    const totalAmountInput = document.getElementById('totalAmount');

    if (hoursWorkedInput && totalAmountInput) {
        // Get hourly rate from data attribute or hidden field
        const hourlyRate = parseFloat(hoursWorkedInput.getAttribute('data-hourly-rate')) ||
            parseFloat(document.getElementById('hourlyRate')?.value) || 0;

        hoursWorkedInput.addEventListener('input', function () {
            const hours = parseFloat(this.value) || 0;
            const total = hours * hourlyRate;

            if (totalAmountInput) {
                totalAmountInput.value = total.toFixed(2);
            }

            // Validation feedback
            if (hours > 180) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else if (hours > 0) {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
            } else {
                this.classList.remove('is-invalid', 'is-valid');
            }
        });
    }
}

// Initialize all functionality when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    initializeThemeToggle();
    initializeSearch();
    initializeSearchPlaceholder();
    handleSearchErrors();
    initializeClaimAutoCalculation();

    console.log('CMCS Application JavaScript initialized successfully');
});

// Utility function to show notifications (can be used throughout the app)
function showNotification(message, type = 'info') {
    // You can implement toast notifications here if needed
    console.log(`${type.toUpperCase()}: ${message}`);
}