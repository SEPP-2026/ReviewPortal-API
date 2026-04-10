import Link from "next/link";
import { ArrowLeft } from "lucide-react";

export default function EquipmentNotFound() {
  return (
    <div className="min-h-screen bg-[#F2F2F2] flex items-center justify-center">
      <div className="text-center">
        <div className="text-6xl mb-4">🔧</div>
        <h1 className="text-3xl font-bold text-[#111111] mb-4">
          Equipment Not Found
        </h1>
        <p className="text-[#666666] mb-8">
          The equipment you're looking for doesn't exist or has been removed.
        </p>
        <Link
          href="/equipment"
          className="inline-flex items-center gap-2 bg-accent hover:bg-[#C97F00] text-black font-semibold px-6 py-3 rounded-lg transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
          Browse Equipment
        </Link>
      </div>
    </div>
  );
}
