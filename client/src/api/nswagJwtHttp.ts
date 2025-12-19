import { getAuthToken } from "./authService";
import { toast } from "react-toastify";

type NswagJwtHttp = {
    fetch(url: RequestInfo, init?: RequestInit): Promise<Response>;
};

type ProblemJson = {
    detail?: unknown;
    title?: unknown;
};

function pickProblemMessage(data: unknown): string | null {
    if (!data || typeof data !== "object") return null;

    const obj = data as ProblemJson;

    const detail = typeof obj.detail === "string" ? obj.detail : null;
    const title = typeof obj.title === "string" ? obj.title : null;

    return detail ?? title;
}

async function tryGetProblemMessage(res: Response): Promise<string | null> {
    const ct = res.headers.get("content-type") ?? "";
    try {
        if (ct.includes("application/problem+json") || ct.includes("application/json")) {
            const data: unknown = await res.json();
            return pickProblemMessage(data);
        }
        const text = await res.text();
        return text.trim() ? text : null;
    } catch {
        return null;
    }
}


export const createJwtHttp = (): NswagJwtHttp => ({
    fetch: async (url, init = {}) => {
        const token = getAuthToken();
        const headers = new Headers(init.headers);

        if (token) {
            headers.set("Authorization", `Bearer ${token}`);
        }
        if (!headers.has("Content-Type")) headers.set("Content-Type", "application/json");
        try {
            const response = await window.fetch(url, {...init, headers })
            if (!response.ok) {
                const clone = response.clone();
                const msg = (await tryGetProblemMessage(clone)) ?? `Request failed (${response.status})`

                toast.error(msg);
                return response;
            }
            return response;
        } catch (error: unknown) {
            toast.error("Network error, please try again");
            throw error;
        }
    },
});