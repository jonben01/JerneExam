import { Navigate } from "react-router-dom";
import type { ReactNode } from "react";
import { isAdmin, isUserLoggedIn } from "../api/authService";

interface ProtectedAdminProps {
    children: ReactNode;
}

const ProtectedAdmin = ({ children }: ProtectedAdminProps) => {
    const isLoggedIn = isUserLoggedIn();
    const isUserAdmin = isAdmin();

    if (!isLoggedIn) {
        return <Navigate to="/login" replace />;
    }
    if (!isUserAdmin) {
        return <Navigate to="/" replace />;
    }

    return <>{children}</>;
};

export default ProtectedAdmin;