import * as React from "react";
import { PropsWithChildren } from "react";

export interface GlobalPageLayoutProps extends PropsWithChildren {
  renderSidebar?: () => React.ReactNode;
  renderSearch?: () => React.ReactNode;
}

export const GlobalPageLayout: React.FC<GlobalPageLayoutProps> = ({
  renderSidebar,
  renderSearch,
  children,
}) => {
  return (
    <>
      <div className="hidden md:block min-h-screen">
        <div className="border-t min-h-screen">
          <div className="bg-background min-h-screen">
            <div className="flex min-h-screen">
              {renderSidebar && (
                <aside
                  className={
                    "fixed left-0 top-0 h-screen w-64 bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 z-10"
                  }
                >
                  {renderSidebar?.()}
                </aside>
              )}
              <main
                className={`flex-1 relative lg:border-l pb-24 ${renderSidebar ? "ml-64" : ""}`}
              >
                {renderSearch && (
                  <div className="pl-64 fixed top-4 left-8 right-8 z-10">
                    {renderSearch()}
                  </div>
                )}
                {children}
              </main>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};
