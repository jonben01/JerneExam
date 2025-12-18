import { Navigate } from "react-router-dom";
import { isUserLoggedIn } from "../api/authService";

interface ProtectedRouteProps {
    children: React.ReactNode;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
    const isLoggedIn = isUserLoggedIn();

    if (!isLoggedIn) {
        return (
            <Navigate
                to='/login'
                replace
            />
        );
    }

    return <>{children}</>;
};

export default ProtectedRoute;