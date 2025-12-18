import { loginUser } from "../../api/authService";
import {useNavigate} from "react-router-dom";

const LoginPage = () => {

    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        const { password, email } = e.currentTarget;

        try {
            const response = await loginUser(email.value, password.value);
            console.log("Login successful:", response);
            navigate("/")
        } catch (err) {
            console.error("Login error:", err);
        }
    };

    return (
        <div className='login-container'>
            <div className='login-card'>
                <h1>Login</h1>
                <form onSubmit={handleSubmit}>
                    <div className='form-group'>
                        <label htmlFor='email'>Email:</label>
                        <input
                            type='email'
                            id='email'
                            name='email'
                            required
                            placeholder='Enter your email'
                        />
                    </div>
                    <div className='form-group'>
                        <label htmlFor='password'>Password:</label>
                        <input
                            type='password'
                            id='password'
                            name='password'
                            required
                            placeholder='Enter your password'
                        />
                    </div>
                    <button
                        type='submit'
                        className='submit-btn'
                    >
                        Submit
                    </button>
                </form>
            </div>
        </div>
    );
};

export default LoginPage;