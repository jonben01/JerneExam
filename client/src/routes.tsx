import { Route, Routes } from "react-router-dom";
import LoginPage from "./pages/login";
import App from "./App";
import Navbar from "./components/Navbar.tsx";
import ProtectedRoute from "./components/ProtectedRoute.tsx";
import GamePage from "./pages/game";
import {ToastContainer} from "react-toastify";
import AdminPage from "./pages/admin";
import TransactionsPage from "./pages/admin/transactions";
import ProtectedAdmin from "./components/ProtectedAdmin.tsx";
import UserPage from "./pages/user";

const AppRoutes = () => {
    return (
        <>
        <Navbar />
        <Routes>
            <Route path='/' element={<App/>}/>

            <Route path='/login' element={<LoginPage />}/>

            <Route path='/game' element={
                    <ProtectedRoute>
                        <GamePage/>
                    </ProtectedRoute>}/>

            <Route
                path="/admin"
                element={
                    <ProtectedAdmin>
                        <AdminPage />
                    </ProtectedAdmin>
                }
            ></Route>
            <Route
                path="/admin/transactions"
                element={
                    <ProtectedAdmin>
                        <TransactionsPage />
                    </ProtectedAdmin>
                }
            />
            <Route
                path="/user"
                element={
                    <ProtectedRoute>
                        <UserPage />
                    </ProtectedRoute>
                }
            />
        </Routes>
            <ToastContainer position="top-center" autoClose={3000} />
    </>
    );
};

export default AppRoutes;