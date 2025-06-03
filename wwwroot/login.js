const loginBtn = document.getElementById('loginBtn');
const loginMessage = document.getElementById('loginMessage');

loginBtn.addEventListener('click', async () => {
  const email = document.getElementById('loginEmail').value.trim();
  const password = document.getElementById('loginPassword').value;

  loginMessage.textContent = ''; // Clear old messages

  if (!email || !password) {
    loginMessage.style.color = 'red';
    loginMessage.textContent = 'Please fill in all fields.';
    return;
  }

  loginBtn.disabled = true;
  loginBtn.textContent = 'Logging in...';

  try {
    const response = await fetch('/api/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, passwordHash: password })  // match your backend model!
    });

    if (response.ok) {
      loginMessage.style.color = 'green';
      loginMessage.textContent = 'Login successful!';
      // Optional: redirect user here after login
    } else if (response.status === 401) {
      loginMessage.style.color = 'red';
      loginMessage.textContent = 'Invalid email or password.';
    } else {
      loginMessage.style.color = 'red';
      loginMessage.textContent = 'Login failed. Please try again.';
    }
  } catch (error) {
    loginMessage.style.color = 'red';
    loginMessage.textContent = 'Network error. Please check your connection.';
  } finally {
    loginBtn.disabled = false;
    loginBtn.textContent = 'Login';
  }
});
