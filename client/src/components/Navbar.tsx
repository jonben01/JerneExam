import { useMemo } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { isAdmin, isUserLoggedIn, logoutUser } from "../api/authService";

const Navbar = () => {
    const navigate = useNavigate();
    const location = useLocation();

    const isLoggedIn = useMemo(() => isUserLoggedIn(), [location.pathname]);
    const isUserAdmin = useMemo(() => isAdmin(), [location.pathname]);

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
                position: "absolute",
                top: 0,
                left: 0,
                right: 0,
                display: "flex",
                height: "40px",
                justifyContent: "space-between",
                alignItems: "center",
                padding: "1rem 2rem",
                backgroundColor: "#f44336",
                color: "white",
                zIndex: 1000,
            }}
        >
            <div
                style={{ fontSize: "1.5rem", fontWeight: "bold", cursor: "pointer" }}
                onClick={() => navigate("/")}
            >
                Home
            </div>
            {isLoggedIn && !isUserAdmin && (
                <div
                    style={{
                        display: "flex",
                        gap: "50px",
                    }}
                >
                    <h3 onClick={() => navigate("/game")} style={{ cursor: "pointer" }}>
                        Game
                    </h3>{" "}
                    <h3 onClick={() => navigate("/user")} style={{ cursor: "pointer" }}>
                        Manage balance
                    </h3>
                </div>
            )}
            {isUserAdmin && (
                <>
                    <h3 onClick={() => navigate("/admin")} style={{ cursor: "pointer" }}>
                        Admin
                    </h3>{" "}
                    <h3
                        onClick={() => navigate("/admin/transactions")}
                        style={{ cursor: "pointer" }}
                    >
                        Transactions
                    </h3>
                </>
            )}

            {isLoggedIn ? (
                <button
                    style={{
                        padding: "0.5rem 1.5rem",
                        backgroundColor: "white",
                        color: "black",
                        border: "1px solid #ccc",
                        borderRadius: "4px",
                        cursor: "pointer",
                        fontSize: "1rem",
                    }}
                    onClick={handleLogout}
                >
                    Logout
                </button>
            ) : (
                <button
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
                </button>
            )}
        </nav>
    );
};
export default Navbar;