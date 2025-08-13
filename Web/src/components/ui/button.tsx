import React, { ComponentType } from "react";
import { Slot } from "@radix-ui/react-slot";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";
import { LoaderCircle } from "lucide-react";

const buttonVariants = cva(
  "inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm gap-2 " +
    "font-semibold ring-offset-background transition-colors " +
    "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 " +
    "disabled:pointer-events-none disabled:opacity-50 cursor-pointer",
  {
    variants: {
      variant: {
        default:
          "bg-green-600 text-black hover:bg-green-500 focus-visible:outline focus-visible:outline-offset-2 focus-visible:outline-green-600",
        blue:
          "bg-blue-600 text-white hover:bg-blue-500 focus-visible:outline focus-visible:outline-offset-2 focus-visible:outline-blue-600",
        destructive:
          "bg-red-700 text-white hover:bg-red-600 focus-visible:outline focus-visible:outline-offset-2 focus-visible:outline-red-700",
        outline:
          "border border-input bg-background hover:bg-accent hover:text-accent-foreground",
        secondary:
          "bg-secondary text-secondary-foreground hover:bg-secondary/80",
        ghost: "hover:bg-accent hover:text-accent-foreground",
        link: "text-primary underline-offset-4 hover:underline",
      },
      size: {
        default: "h-10 px-4 py-2",
        sm: "h-9 rounded-md px-3",
        lg: "h-11 rounded-md px-8",
        icon: "h-10 w-10",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  },
);

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
  loading?: boolean;
  iconLeft?: ComponentType<{ className: string }>;
  iconRight?: ComponentType<{ className: string }>;
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      iconLeft: IconLeft,
      iconRight: IconRight,
      loading,
      className,
      variant,
      size,
      asChild = false,
      children,
      ...props
    },
    ref,
  ) => {
    const Comp = asChild ? Slot : "button";

    const content = (
      <>
        {IconLeft && !loading ? <IconLeft className="h-4" /> : null}
        {loading && <LoaderCircle className="animate-spin size-4" />}
        {children}
        {IconRight && !loading ? <IconRight className="h-4" /> : null}
      </>
    );

    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        {...props}
      >
        {asChild ? <div className={"flex"}>{content}</div> : content}
      </Comp>
    );
  },
);

Button.displayName = "Button";

export { Button };
