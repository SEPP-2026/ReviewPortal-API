# ReviewPortal Frontend

A modern Next.js 15 application built with React 19, TypeScript, and TailwindCSS 4.

## 🚀 Tech Stack

### Core
- **Next.js** 15.5.9 (App Router)
- **React** 19.0.1
- **TypeScript** 5
- **TailwindCSS** 4.1.12

### UI Components
- **Radix UI** - Accessible component primitives
  - Alert Dialog, Avatar, Checkbox, Collapsible, Dialog
  - Dropdown Menu, Icons, Label, Popover, Progress
  - Radio Group, Scroll Area, Select, Separator
  - Slot, Switch, Tabs, Tooltip
- **Lucide React** 0.511.0 - Icon library
- **cmdk** - Command menu component
- **Sonner** 2.0.7 - Toast notifications

### Forms & Validation
- **React Hook Form** 7.62.0
- **Zod** 3.25.76
- **@hookform/resolvers** - Form validation
- **input-otp** - OTP input component
- **react-day-picker** - Date picker

### State Management & Utilities
- **Zustand** 5.0.8 - State management
- **date-fns** 4.1.0 - Date utilities
- **libphonenumber-js** 1.12.27 - Phone number validation
- **class-variance-authority** - Variant utilities
- **clsx** & **tailwind-merge** - Styling utilities

### Additional Features
- **Sharp** 0.34.3 - Image optimization
- **Recharts** - Charts & data visualization
- **react-quill-new** - Rich text editor
- **react-hotkeys-hook** - Keyboard shortcuts
- **tunnel-rat** 0.1.2 - Portal utilities

### Development
- **ESLint** 9
- **Prettier** 3.6.2
- **PostCSS**

## 📁 Project Structure

```
frontend/
├── app/                    # Next.js app directory
│   ├── globals.css        # Global styles
│   ├── layout.tsx         # Root layout
│   └── page.tsx           # Home page
├── components/            # React components
│   └── ui/               # Reusable UI components
├── hooks/                 # Custom React hooks
│   └── use-debounce.ts   # Debounce hook
├── lib/                   # Utility libraries
│   ├── api-client.ts     # API client for .NET backend
│   └── utils.ts          # Utility functions (cn, etc.)
├── store/                 # Zustand stores
│   └── example-store.ts  # Example store
├── types/                 # TypeScript type definitions
│   └── api.ts            # API types
├── public/                # Static assets
├── .env.example          # Environment variables template
├── components.json       # Component configuration
├── next.config.ts        # Next.js configuration
├── postcss.config.mjs    # PostCSS configuration
├── tsconfig.json         # TypeScript configuration
└── package.json          # Dependencies

```

## 🛠️ Getting Started

### Prerequisites
- Node.js 18.17 or later
- npm, yarn, or pnpm
- .NET backend running (default: `http://localhost:5000/api`)
- MySQL database

### Installation

1. **Install dependencies:**
   ```bash
   npm install
   ```

2. **Set up environment variables:**
   ```bash
   cp .env.example .env.local
   ```
   
   Edit `.env.local` with your configuration:
   ```env
   NEXT_PUBLIC_API_URL=http://localhost:5000/api
   NEXT_PUBLIC_APP_URL=http://localhost:3000
   ```

3. **Run the development server:**
   ```bash
   npm run dev
   ```

4. **Open your browser:**
   Navigate to [http://localhost:3000](http://localhost:3000)

## 📝 Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run start` - Start production server
- `npm run lint` - Run ESLint
- `npm run format` - Format code with Prettier

## 🎨 Styling

This project uses **TailwindCSS 4** with CSS variables for theming. The design system includes:

- Light and dark mode support
- Customizable color scheme
- Consistent spacing and typography
- Accessible components

### Using the `cn()` utility
```typescript
import { cn } from "@/lib/utils";

<div className={cn("base-class", condition && "conditional-class")} />
```

## 🔌 API Integration

The project includes a pre-configured API client for communicating with the .NET backend:

```typescript
import { apiClient } from "@/lib/api-client";

// GET request
const response = await apiClient.get("/endpoint");

// POST request
const response = await apiClient.post("/endpoint", { data });

// With authentication
const response = await apiClient.get("/protected", { 
  token: "your-jwt-token" 
});
```

## 📦 State Management

Using Zustand for state management:

```typescript
import { create } from "zustand";

interface Store {
  data: string;
  setData: (data: string) => void;
}

export const useStore = create<Store>((set) => ({
  data: "",
  setData: (data) => set({ data }),
}));
```

## 🧩 Adding UI Components

This project follows the shadcn/ui pattern. To add components:

1. Create component in `components/ui/`
2. Use Radix UI primitives
3. Style with TailwindCSS
4. Export from component file

Example button component structure:
```typescript
import * as React from "react";
import { Slot } from "@radix-ui/react-slot";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";

// Define variants, create component...
```

## 🔧 Configuration Files

- **next.config.ts** - Next.js configuration with image optimization
- **tsconfig.json** - TypeScript compiler options
- **postcss.config.mjs** - PostCSS with TailwindCSS 4
- **components.json** - Component library configuration
- **.prettierrc** - Code formatting rules
- **eslint.config.mjs** - Linting rules

## 🚢 Deployment

### Build for Production
```bash
npm run build
npm run start
```

### Deploy to Vercel
```bash
vercel
```

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Run linting and type checks
4. Submit a pull request

## 📄 License

This project is part of the ReviewPortal system.

## 🔗 Backend Integration

This frontend is designed to work with a **.NET backend** and **MySQL database**. Ensure your backend API is running and accessible at the URL specified in your environment variables.

### API Endpoints
Configure your API base URL in `.env.local`:
```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

### Authentication
The API client supports JWT token authentication. Pass the token in the request options:
```typescript
const response = await apiClient.get("/user/profile", { 
  token: userToken 
});
```

## 📚 Learn More

- [Next.js Documentation](https://nextjs.org/docs)
- [React Documentation](https://react.dev)
- [TailwindCSS Documentation](https://tailwindcss.com/docs)
- [Radix UI Documentation](https://www.radix-ui.com/docs/primitives)
- [Zustand Documentation](https://docs.pmnd.rs/zustand)

