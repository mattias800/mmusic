import * as React from "react";
import { PropsWithChildren } from "react";
import { Sidebar } from "@/features/sidebar/Sidebar.tsx";

export interface PageLayoutProps extends PropsWithChildren {}

export const PageLayout: React.FC<PageLayoutProps> = ({ children }) => {
  return (
    <>
      <div className="hidden md:block h-full">
        <div className="border-t h-full">
          <div className="bg-background h-full">
            <div className="grid lg:grid-cols-5 h-full">
              <Sidebar className="hidden lg:block" />
              <div className="col-span-3 lg:col-span-4 lg:border-l">
                <div className="h-full px-4 py-6 lg:px-8">{children}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};
