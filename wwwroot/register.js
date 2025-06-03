const registerBtn = document.getElementById('registerBtn');
const registerMessage = document.getElementById('registerMessage');

registerBtn.addEventListener('click', async () => {
  const email = document.getElementById('registerEmail').value.trim();
  const password = document.getElementById('registerPassword').value;

  registerMessage.textContent = '';
  if (!email || !password) {
    registerMessage.style.color = 'red';
    registerMessage.textContent = 'Please fill in all fields.';
    return;
  }
  if (password.length < 6) {
    registerMessage.style.color = 'red';
    registerMessage.textContent = 'Password must be at least 6 characters.';
    return;
  }

  registerBtn.disabled = true;
  registerBtn.textContent = 'Registering...';

  try {
    const response = await fetch('/api/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, passwordHash: password })
    });

    if (response.ok) {
      registerMessage.style.color = 'green';
      registerMessage.textContent = 'Registration successful! You can now login.';
      // Optionally clear inputs
      document.getElementById('registerEmail').value = '';
      document.getElementById('registerPassword').value = '';
    } else if (response.status === 409) {
      registerMessage.style.color = 'red';
      registerMessage.textContent = 'Email already registered.';
    } else {
      registerMessage.style.color = 'red';
      registerMessage.textContent = 'Registration failed. Try again.';
    }
  } catch (error) {
    registerMessage.style.color = 'red';
    registerMessage.textContent = 'Network error. Check your connection.';
  } finally {
    registerBtn.disabled = false;
    registerBtn.textContent = 'Register';
  }
});
