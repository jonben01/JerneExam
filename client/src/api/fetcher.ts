export const getData = async (url: string) => {
    try {
        const response = await fetch(url, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        });

        return await response.json();
    } catch (error) {
        throw error;
    }
};

export const postData = async (url: string, request: unknown) => {
    try {
        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: request ? JSON.stringify(request) : undefined,
        });

        return await response.json();
    } catch (error) {
        throw error;
    }
};