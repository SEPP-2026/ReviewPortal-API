import { Button } from "@/components/ui/button";
import { CheckCircle2 } from "lucide-react";

export default function Home() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center p-8">
      <main className="flex max-w-4xl flex-col items-center gap-8 text-center">
        <div className="space-y-4">
          <h1 className="text-4xl font-bold tracking-tight sm:text-6xl">
            ReviewPortal
          </h1>
          <p className="text-xl text-slate-600 dark:text-slate-400">
            Next.js 15 + React 19 + TypeScript + TailwindCSS 4
          </p>
        </div>

        <div className="grid w-full max-w-2xl gap-4 sm:grid-cols-2">
          <div className="rounded-lg border border-slate-200 bg-white p-6 text-left dark:border-slate-800 dark:bg-slate-950">
            <h3 className="mb-2 font-semibold">Frontend Stack</h3>
            <ul className="space-y-2 text-sm text-slate-600 dark:text-slate-400">
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                Next.js 15.5.9
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                React 19.0.1
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                TailwindCSS 4.1.12
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                TypeScript 5
              </li>
            </ul>
          </div>

          <div className="rounded-lg border border-slate-200 bg-white p-6 text-left dark:border-slate-800 dark:bg-slate-950">
            <h3 className="mb-2 font-semibold">Backend Integration</h3>
            <ul className="space-y-2 text-sm text-slate-600 dark:text-slate-400">
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                .NET Backend
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                MySQL Database
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                API Client Ready
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                Type-safe Requests
              </li>
            </ul>
          </div>

          <div className="rounded-lg border border-slate-200 bg-white p-6 text-left dark:border-slate-800 dark:bg-slate-950">
            <h3 className="mb-2 font-semibold">UI Components</h3>
            <ul className="space-y-2 text-sm text-slate-600 dark:text-slate-400">
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                Radix UI Primitives
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                Lucide Icons
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                React Hook Form
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                Zod Validation
              </li>
            </ul>
          </div>

          <div className="rounded-lg border border-slate-200 bg-white p-6 text-left dark:border-slate-800 dark:bg-slate-950">
            <h3 className="mb-2 font-semibold">State & Utilities</h3>
            <ul className="space-y-2 text-sm text-slate-600 dark:text-slate-400">
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                Zustand Store
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                Date-fns Utilities
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                Toast Notifications
              </li>
              <li className="flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                Rich Text Editor
              </li>
            </ul>
          </div>
        </div>

        <div className="flex gap-4">
          <Button size="lg">Get Started</Button>
          <Button variant="outline" size="lg">
            View Documentation
          </Button>
        </div>

        <div className="mt-8 rounded-lg border border-slate-200 bg-slate-50 p-4 dark:border-slate-800 dark:bg-slate-900">
          <p className="text-sm text-slate-600 dark:text-slate-400">
            <strong>Next steps:</strong> Configure your .NET backend URL in{" "}
            <code className="rounded bg-slate-100 px-2 py-1 font-mono text-xs dark:bg-slate-800">
              .env.local
            </code>
          </p>
        </div>
      </main>
    </div>
  );
}
