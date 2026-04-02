// api.js — shared helper for all pages
const IS_LOCAL = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';
const API_BASE = IS_LOCAL 
  ? 'http://localhost:5000/api' 
  : (typeof APP_CONFIG !== 'undefined' ? APP_CONFIG.apiUrl : 'https://k53-prep-app-production.up.railway.app/api'); 

function getAdminCode() {
  return sessionStorage.getItem('adminCode') || '';
}

function getDeviceId() {
  let deviceId = localStorage.getItem('k53_device_id');
  if (!deviceId) {
    deviceId = 'device_' + Math.random().toString(36).substr(2, 9) + '_' + Date.now();
    localStorage.setItem('k53_device_id', deviceId);
  }
  return deviceId;
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

// Request permission to flip a card
async function requestCardFlip(studentId) {
    const res = await apiFetch(`/students/${studentId}/flip`, { method: 'POST' });
    return res; // { allowed: true/false, remaining: int }
}

// Request permission to change card
async function requestCardNext(studentId) {
    const res = await apiFetch(`/students/${studentId}/next`, { method: 'POST' });
    return res; // { allowed: true/false, remaining: int }
}

class PaymentService {
    // Initialize checkout flow 
    // Returns PayFast URL and form fields so we can auto-submit them
    static async createCheckout(studentId) {
        const response = await fetch(`${API_BASE}/payments/student/${studentId}/checkout`, {
            method: 'POST'
        });
        const data = await response.json();
        if (!response.ok) throw new Error(data.message || 'Checkout failed');
        return data; // { url, fields }
    }
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
