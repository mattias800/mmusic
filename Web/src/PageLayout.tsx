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
      <div className="hidden md:block h-full">
        <div className="border-t h-full">
          <div className="bg-background h-full">
            <div className="flex h-full">
              {renderSidebar && (
                <aside
                  className={"h-screen w-64 flex-shrink-0"}
                >
                  {renderSidebar?.()}
                </aside>
              )}
              <main className="flex-1 lg:border-l pb-24">
                {children}
              </main>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};
