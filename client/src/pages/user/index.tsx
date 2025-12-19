import { useState } from "react";
import { createDepositRequest } from "../../api/transactionService";
import { toast } from "react-toastify";

const UserPage = () => {
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        const { amountDkk, mobilePayReference } = e.currentTarget;

        const amount = Number(amountDkk.value);
        if (isNaN(amount) || amount <= 0) {
            toast.error("Please enter a valid amount");
            return;
        }

        if (!mobilePayReference.value.trim()) {
            toast.error("Please enter a MobilePay reference");
            return;
        }

        setIsSubmitting(true);
        await createDepositRequest(amount, mobilePayReference.value.trim());
        toast.success("Deposit request submitted successfully!");
        setIsSubmitting(false);
        e.currentTarget.reset();
    };

    return (
        <div style={{ padding: "24px", maxWidth: "600px", margin: "0 auto" }}>
            <h2 style={{ marginBottom: "24px" }}>Create Deposit Request</h2>

            <form
                onSubmit={handleSubmit}
                style={{ display: "flex", flexDirection: "column", gap: "16px" }}
            >
                <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                    <label
                        htmlFor="amount"
                        style={{ fontWeight: "500", textAlign: "left" }}
                    >
                        Amount (DKK)
                    </label>
                    <input
                        name="amountDkk"
                        id="amount"
                        type="number"
                        placeholder="Enter amount"
                        style={{
                            padding: "12px",
                            border: "1px solid #ddd",
                            borderRadius: "4px",
                            fontSize: "16px",
                        }}
                        disabled={isSubmitting}
                    />
                </div>

                <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                    <label
                        htmlFor="reference"
                        style={{ fontWeight: "500", textAlign: "left" }}
                    >
                        MobilePay Reference
                    </label>
                    <input
                        name="mobilePayReference"
                        id="reference"
                        type="text"
                        placeholder="Enter MobilePay reference"
                        style={{
                            padding: "12px",
                            border: "1px solid #ddd",
                            borderRadius: "4px",
                            fontSize: "16px",
                        }}
                        disabled={isSubmitting}
                    />
                </div>

                <button
                    type="submit"
                    disabled={isSubmitting}
                    style={{
                        padding: "12px 24px",
                        backgroundColor: isSubmitting ? "#ccc" : "#4CAF50",
                        color: "white",
                        border: "none",
                        borderRadius: "4px",
                        cursor: isSubmitting ? "not-allowed" : "pointer",
                        fontWeight: "bold",
                        fontSize: "16px",
                        marginTop: "8px",
                    }}
                >
                    {isSubmitting ? "Submitting..." : "Submit Deposit Request"}
                </button>
            </form>
        </div>
    );
};

export default UserPage;