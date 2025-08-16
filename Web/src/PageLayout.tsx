import * as React from "react";
import { PropsWithChildren } from "react";

export interface PageLayoutProps extends PropsWithChildren {
  renderSidebar?: () => React.ReactNode;
}

export const PageLayout: React.FC<PageLayoutProps> = ({
  renderSidebar,
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
                  className={"fixed left-0 top-0 h-screen w-64 bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 z-10"}
                >
                  {renderSidebar?.()}
                </aside>
              )}
              <main className={`flex-1 lg:border-l pb-24 bg-background ${renderSidebar ? 'ml-64' : ''}`}>
                {children}
              </main>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};
