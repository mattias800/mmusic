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
              <div className="w-64"/>
              <aside className={"h-screen fixed top-0 left-0 bottom-0 w-64"}>
                <Sidebar />
              </aside>
              <main className="col-span-3 lg:col-span-4 lg:border-l">
                <div className="h-full px-4 py-6 lg:px-8">{children}</div>
              </main>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};
