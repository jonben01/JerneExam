import {
    AuthClient,
    type LoginRequest,
    type LoginResponse,
} from "../generated-ts-client";
import {toast} from "react-toastify";

export const API_BASE_URL =
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5237";

const authClient = new AuthClient(API_BASE_URL);

const TOKEN_KEY = "authToken";
const USER_KEY = "user";
const ROLE_KEY = "role";
const EXPIRES_KEY = "authExpiresUtc";

export const loginUser = async (
    email: string,
    password: string
): Promise<LoginResponse> => {
    try {
        const request: LoginRequest = {
            email,
            password,
        };

        const response = await authClient.login(request);

        // Store token in localStorage if login is successful
        if (response.token) {

            localStorage.setItem(TOKEN_KEY, response.token);

            if (response.user) {
                localStorage.setItem(USER_KEY, JSON.stringify(response.user));
            }

            if (response.role) {
                localStorage.setItem(ROLE_KEY, response.role);
            }

            if (response.expires) {
                localStorage.setItem(EXPIRES_KEY, response.expires);
            }

        } else {
            logoutUser();
        }

        return response;

    } catch (error: unknown) {
        toast.error("Wrong credentials");
        console.error("Login failed:", error);
        throw error;
    }
};

export const logoutUser = () => {
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(ROLE_KEY);
    localStorage.removeItem(EXPIRES_KEY);
};

export const getAuthToken = (): string | null => {
    return localStorage.getItem(TOKEN_KEY);
};

export const getRole = (): string | null => localStorage.getItem(ROLE_KEY);

const getExpiresUtcMs = (): number | null => {
    const expiresIso = localStorage.getItem(EXPIRES_KEY);
    if (!expiresIso) return null;

    const ms = Date.parse(expiresIso);
    return Number.isFinite(ms) ? ms : null;
}

export const isUserLoggedIn = (): boolean => {
    const token = getAuthToken();
    if (!token) return false;

    const expiresMs = getExpiresUtcMs();
    if (!expiresMs) {
        logoutUser();
        return false;
    }
    //1 minute grace
    const skewMs = 60_000;
    const ok = Date.now() + skewMs < expiresMs;

    if (!ok) logoutUser();
    return ok;
};

export const isAdmin = (): boolean => {
    if (!isUserLoggedIn()) return false;
    return getRole() === "Admin";
};