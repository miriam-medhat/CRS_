// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function applyTheme() {
    const isDark = localStorage.getItem('theme') === 'dark';
    document.body.classList.toggle('dark-theme', isDark);
    const toggle = document.getElementById('themeToggleSwitch');
    if (toggle) toggle.checked = isDark;
}

function toggleTheme() {
    const isDark = document.body.classList.toggle('dark-theme');
    localStorage.setItem('theme', isDark ? 'dark' : 'light');
    const toggle = document.getElementById('themeToggleSwitch');
    if (toggle) toggle.checked = isDark;
}

document.addEventListener('DOMContentLoaded', applyTheme);

