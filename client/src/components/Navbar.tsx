import {isUserLoggedIn, logoutUser} from "../api/authService.ts";
import {useLocation, useNavigate} from "react-router-dom";
import {useMemo} from "react";

const Navbar = () => {
    const navigate = useNavigate();
    const location = useLocation();

    const isLoggedIn = useMemo(() => isUserLoggedIn(), [location.pathname]);

    const handleLogout = () => {
        logoutUser();
        navigate("/login");
    };

    const handleLogin = () => {
        navigate("/login");

    };
    return (
        <nav
            style={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
                padding: "1rem 2rem",
                backgroundColor: "#f44336",
                color: "white",
                position: "fixed",
                top: 0,
                left: 0,
                right: 0,
                zIndex: 1000,
            }}
        >
            <div
                style={{ fontSize: "1.5rem", fontWeight: "bold", cursor: "pointer" }}
                onClick={() => navigate("/")}
            >
                Home
            </div>
            {isLoggedIn && (
                <div>
                    <h2
                        onClick={() => navigate("/game")}
                        style={{ cursor: "pointer" }}
                    >
                        Game
                    </h2>
                </div>
            )}
            {isLoggedIn ? <button style={{
                padding: "0.5rem 1.5rem",
                backgroundColor: "#007bff",
                color: "white",
                border: "none",
                borderRadius: "4px",
                cursor: "pointer",
                fontSize: "1rem",
            }} onClick={handleLogout} >logout</button> : <button
                style={{
                    padding: "0.5rem 1.5rem",
                    backgroundColor: "#007bff",
                    color: "white",
                    border: "none",
                    borderRadius: "4px",
                    cursor: "pointer",
                    fontSize: "1rem",
                }}
                onClick={handleLogin}
            >
                Login
            </button>}
        </nav>
    );
};
export default Navbar;