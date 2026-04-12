import Link from "next/link";
import { ArrowLeft, Wrench } from "lucide-react";

export default function EquipmentNotFound() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-[#F2F2F2]">
      <div className="text-center">
        <div className="mb-4 flex justify-center text-accent">
          <Wrench className="h-16 w-16" aria-hidden="true" />
        </div>
        <h1 className="mb-4 text-3xl font-bold text-[#111111]">
          Equipment Not Found
        </h1>
        <p className="mb-8 text-[#666666]">
          The equipment you&apos;re looking for doesn&apos;t exist or has been
          removed.
        </p>
        <Link
          href="/equipment"
          className="inline-flex items-center gap-2 rounded-lg bg-accent px-6 py-3 font-semibold text-black transition-colors hover:bg-[#C97F00]"
        >
          <ArrowLeft className="h-5 w-5" />
          Browse Equipment
        </Link>
      </div>
    </div>
  );
}
