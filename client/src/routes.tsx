import { Route, Routes } from "react-router-dom";
import LoginPage from "./pages/login";
import App from "./App";
import Navbar from "./components/Navbar.tsx";
import ProtectedRoute from "./components/ProtectedRoute.tsx";
import GamePage from "./pages/game";
import {ToastContainer} from "react-toastify";

const AppRoutes = () => {
    return (
        <>
        <Navbar />
        <Routes>
            <Route
                path='/'
                element={<App/>}
            />
            <Route
                path='/login'
                element={<LoginPage />}
            />
            <Route
                path='/game'
                element={
                    <ProtectedRoute>
                        <GamePage/>
                    </ProtectedRoute>
                }
            />
        </Routes>
            <ToastContainer position="top-center" autoClose={3000} />
    </>
    );
};

export default AppRoutes;