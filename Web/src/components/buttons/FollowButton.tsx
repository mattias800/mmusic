import * as React from "react";

export interface FollowButtonProps {}

export const FollowButton: React.FC<FollowButtonProps> = () => {
  return (
    <button className="border border-white rounded-full px-4 py-1 text-sm hover:bg-white hover:text-black transition">
      Follow
    </button>
  );
};
