import * as React from "react";
import { PropsWithChildren } from "react";

type Variant = "error" | "info" | "warning" | "success";

export interface MessageBoxProps extends PropsWithChildren {
  message: string;
  variant?: Variant;
}

export const MessageBox: React.FC<MessageBoxProps> = ({
  message,
  variant = "info",
  children,
}) => {
  const styles = variantStyles[variant];

  return (
    <div className={`p-3 rounded-md flex flex-col gap-4 ${styles.container}`}>
      <p className={`text-sm ${styles.text}`}>{message}</p>
      <div className={"flex justify-end"}>{children}</div>
    </div>
  );
};

const variantStyles = {
  error: {
    container: "bg-red-900/30 border border-red-700",
    text: "text-red-400",
  },
  info: {
    container: "bg-blue-900/30 border border-blue-700",
    text: "text-blue-100",
  },
  warning: {
    container: "bg-orange-900/30 border border-orange-700",
    text: "text-orange-400",
  },
  success: {
    container: "bg-green-900/30 border border-green-700",
    text: "text-green-400",
  },
};
