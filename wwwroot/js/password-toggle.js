// Password visibility toggle for all forms
function setupPasswordToggles() {
    // Find all password toggle buttons
    document.querySelectorAll('[id^="toggle"]').forEach(button => {
        button.addEventListener('click', function() {
            const buttonId = this.id;
            let passwordFieldId, iconId;
            
            // Determine which field this button toggles
            if (buttonId.includes('RegisterPassword')) {
                passwordFieldId = 'registerPassword';
                iconId = 'registerPasswordIcon';
            } else if (buttonId.includes('RegisterConfirmPassword')) {
                passwordFieldId = 'registerConfirmPassword';
                iconId = 'registerConfirmPasswordIcon';
            } else if (buttonId.includes('LoginPassword')) {
                passwordFieldId = 'loginPassword';
                iconId = 'loginPasswordIcon';
            }
            
            // Toggle visibility
            const passwordField = document.getElementById(passwordFieldId);
            const icon = document.getElementById(iconId);
            
            if (passwordField.type === 'password') {
                passwordField.type = 'text';
                icon.classList.remove('bi-eye');
                icon.classList.add('bi-eye-slash');
            } else {
                passwordField.type = 'password';
                icon.classList.remove('bi-eye-slash');
                icon.classList.add('bi-eye');
            }
        });
    });
}

// Initialize when page loads
document.addEventListener('DOMContentLoaded', setupPasswordToggles);