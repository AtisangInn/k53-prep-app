/**
 * theme.js — Dark mode toggle and persistence for K53 Academy inner pages.
 * Adds/removes the "dark" class on <html> and saves preference to localStorage.
 */

const DARK_KEY = 'k53_dark_mode';

function applyTheme(dark) {
    if (dark) {
        document.documentElement.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
    }
    // Update all toggle button icons on the page
    document.querySelectorAll('.theme-toggle-icon').forEach(el => {
        el.innerHTML = dark
            ? `<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364-.707-.707M6.343 6.343l-.707-.707m12.728 0-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"/></svg>`
            : `<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"/></svg>`;
    });
    document.querySelectorAll('.theme-toggle-label').forEach(el => {
        el.textContent = dark ? 'Light' : 'Dark';
    });
}

function toggleTheme() {
    const isDark = document.documentElement.classList.contains('dark');
    const newDark = !isDark;
    localStorage.setItem(DARK_KEY, newDark ? '1' : '0');
    applyTheme(newDark);
}

// Apply saved preference on load (before paint to avoid flash)
(function () {
    const saved = localStorage.getItem(DARK_KEY);
    // Default to dark if no preference saved (matches landing page theme)
    const preferDark = saved === null ? true : saved === '1';
    applyTheme(preferDark);
})();
