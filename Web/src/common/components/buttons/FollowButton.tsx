import * as React from "react";

export type FollowButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement>;

export const FollowButton: React.FC<FollowButtonProps> = ({ ...props }) => {
  return (
    <button
      className="border border-white rounded-full px-4 py-1 text-sm hover:bg-white hover:text-black transition"
      {...props}
    >
      Follow
    </button>
  );
};
