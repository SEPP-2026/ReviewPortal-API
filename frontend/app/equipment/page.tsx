import { Suspense } from "react";
import { EquipmentCatalogue } from "@/components/equipment";

export const metadata = {
  title: "Equipment Catalogue | Shelton Tool-Hire",
  description:
    "Browse our selection of professional tools and equipment for rent. Construction, landscaping, plumbing, and more.",
};

export default function EquipmentPage() {
  return (
    <Suspense
      fallback={
        <div className="min-h-screen bg-[#F2F2F2] flex items-center justify-center">
          <div className="text-[#111111]">Loading...</div>
        </div>
      }
    >
      <EquipmentCatalogue />
    </Suspense>
  );
}
