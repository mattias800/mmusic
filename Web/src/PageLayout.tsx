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
            <div className="grid lg:grid-cols-5 h-full">
              {renderSidebar && (
                <>
                  <div className="w-64" />
                  <aside
                    className={"h-screen fixed top-0 left-0 bottom-0 w-64"}
                  >
                    {renderSidebar?.()}
                  </aside>
                </>
              )}
              <main className="col-span-3 lg:col-span-4 lg:border-l pb-24">
                {children}
              </main>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};
