// api.js — shared helper for all pages
const IS_LOCAL = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';
const API_BASE = IS_LOCAL 
  ? 'http://localhost:5000/api' 
  : 'https://k53-prep-app-production.up.railway.app/api'; 

function getAdminCode() {
  return sessionStorage.getItem('adminCode') || '';
}

function getStudent() {
  const s = sessionStorage.getItem('student');
  return s ? JSON.parse(s) : null;
}

function requireStudent() {
  const s = getStudent();
  if (!s) { window.location.href = 'index.html'; return null; }
  return s;
}

function requireAdmin() {
  const code = getAdminCode();
  if (!code) { window.location.href = 'index.html'; return null; }
  return code;
}

async function apiFetch(path, options = {}) {
  const res = await fetch(API_BASE + path, {
    headers: {
      'Content-Type': 'application/json',
      'X-Admin-Code': getAdminCode(),
      ...(options.headers || {})
    },
    ...options
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `HTTP ${res.status}`);
  }
  if (res.status === 204) return null;
  return res.json();
}
