/**
 * K53 Prep App Configuration
 * Use this file to customize the app for different driving schools.
 */
const APP_CONFIG = {
    schoolName: "K53 Academy", // Change to the school's name
    shortName: "K53 Academy",
    tagline: "Your journey to the road starts here",
    primaryColor: "#c1272d", // Main brand color
    logoPath: "", // Leave empty to use text logo. Set to "assets/img/logo.png" for a school's logo image.
    handbookPath: "assets/docs/godu-godu-driving-school-k53.pdf", // Path to the PDF handbook
    apiUrl: "https://k53-prep-app-production.up.railway.app/api", // Production API URL
    social: {
        facebook: "",
        whatsapp: "",
        website: ""
    },
    features: {
        allowGuestStudy: true,
        requireProfileForTest: true
    }
};

/**
 * Helper function to apply branding to the page
 */
function applyBranding() {
    // Update Document Title
    document.title = APP_CONFIG.schoolName;

    // Update instances of school name
    document.querySelectorAll('.school-name').forEach(el => {
        el.textContent = APP_CONFIG.schoolName;
    });

    // Update Footer Year & Name
    const footerName = document.getElementById('footer-school-name');
    if (footerName) footerName.textContent = APP_CONFIG.schoolName;

    // Handle the logo on the landing page
    const logoSchoolName = document.getElementById('logo-school-name');
    if (logoSchoolName) logoSchoolName.textContent = APP_CONFIG.schoolName;

    // If a custom logo image is provided, swap the text logo for an image
    if (APP_CONFIG.logoPath) {
        const wrapper = document.getElementById('school-logo-wrapper');
        if (wrapper) {
            wrapper.innerHTML = `<img src="${APP_CONFIG.logoPath}" alt="${APP_CONFIG.schoolName} Logo" class="school-logo w-48 h-48 mx-auto drop-shadow-2xl">`;
        }
    }

    // Apply primary color as a CSS variable
    document.documentElement.style.setProperty('--primary-color', APP_CONFIG.primaryColor);
}

// Automatically apply branding when script is loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', applyBranding);
} else {
    applyBranding();
}
