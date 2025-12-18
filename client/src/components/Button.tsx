import type { ButtonHTMLAttributes } from "react";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    toggled?: boolean;
}

const Button = ({ toggled = false, ...props }: ButtonProps) => {
    const { onClick, children } = props;

    return (
        <button
            {...props}
            onClick={onClick}
            style={{
                width: "150px",
                height: "150px",
                backgroundColor: toggled ? "#dc2626" : "#fecaca",
                color: "black",
                border: toggled ? "2px solid #b91c1c" : "2px solid #fca5a5",
                borderRadius: "8px",
                cursor: "pointer",
                fontSize: "20px",
                fontWeight: "500",
                transition: "all 0.2s ease",
                opacity: toggled ? 1 : 0.5,
                ...props.style,
            }}
        >
            {children}
        </button>
    );
};

export default Button;