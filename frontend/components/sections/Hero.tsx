"use client";

import Link from "next/link";
import { ChevronDown, Search, ArrowRight } from "lucide-react";

export function Hero() {
  return (
    <section className="relative bg-gradient-to-br from-[#FAFAFA] to-[#F2F2F2] pt-[112px] overflow-hidden">
      <div className="max-w-7xl mx-auto px-4">
        <div className="grid lg:grid-cols-2 gap-12 items-center min-h-[calc(100vh-112px)] py-12">

          {/* Left Column */}
          <div className="py-8 lg:py-12">


            {/* Heading */}
            <h1 className="text-5xl md:text-6xl xl:text-7xl font-extrabold text-[#111111] mb-6 leading-[1.05] tracking-tight">
              Rent Quality Tools,{" "}
              <span className="text-accent">Get the Job Done</span>
            </h1>

            {/* Subtitle */}
            <p className="text-[#666666] text-xl mb-10 max-w-lg leading-relaxed">
              Find the perfect tools for your project. No hassle, no technical jargon—just quality equipment delivered when you need it.
            </p>

            {/* Find Equipment Widget - Lighter, friendlier design */}
            <div className="bg-white rounded-3xl p-8 shadow-xl border border-gray-200 max-w-md">
              <div className="flex items-center gap-3 mb-6">
                <div className="w-12 h-12 bg-accent/10 rounded-xl flex items-center justify-center">
                  <Search className="w-6 h-6 text-accent" />
                </div>
                <h3 className="text-[#111111] font-bold text-xl">Find Your Equipment</h3>
              </div>
              <div className="space-y-4 mb-6">
                <div className="relative">
                  <label className="block text-[#666666] text-sm font-medium mb-2">What do you need?</label>
                  <select className="w-full bg-[#F8F8F8] text-[#111111] rounded-xl px-5 py-4 appearance-none focus:outline-none focus:ring-2 focus:ring-accent focus:bg-white transition-all text-base font-medium border border-transparent hover:border-accent/30">
                    <option value="">Select a category...</option>
                    <option>🏗️ Construction Tools</option>
                    <option>🌿 Landscaping & Garden</option>
                    <option>🔧 Plumbing Equipment</option>
                    <option>⚡ Electrical Tools</option>
                    <option>🎨 Painting & Decorating</option>
                    <option>🧹 Cleaning Machines</option>
                  </select>
                  <ChevronDown className="absolute right-4 top-[42px] w-5 h-5 text-[#999] pointer-events-none" />
                </div>
                <div className="relative">
                  <label className="block text-[#666666] text-sm font-medium mb-2">Pick up location</label>
                  <select className="w-full bg-[#F8F8F8] text-[#111111] rounded-xl px-5 py-4 appearance-none focus:outline-none focus:ring-2 focus:ring-accent focus:bg-white transition-all text-base font-medium border border-transparent hover:border-accent/30">
                    <option value="">Choose nearest branch...</option>
                    <option>📍 Shelton Main Store</option>
                    <option>📍 Downtown Branch</option>
                    <option>📍 Industrial Park Location</option>
                  </select>
                  <ChevronDown className="absolute right-4 top-[42px] w-5 h-5 text-[#999] pointer-events-none" />
                </div>
              </div>
              <Link
                href="/equipment"
                className="group flex items-center justify-center gap-2 w-full bg-accent hover:bg-[#C97F00] text-black font-bold py-4 px-6 rounded-xl transition-all duration-200 shadow-lg hover:shadow-xl hover:scale-[1.02]"
              >
                Browse Available Equipment
                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
              </Link>
              <p className="text-center text-[#999] text-sm mt-4">✓ Free delivery over $200 • ✓ Same-day rental available</p>
            </div>
          </div>

          {/* Right Column — Image */}
          <div className="hidden lg:flex items-center justify-center h-full relative">
            <div className="relative">
              <div className="absolute -inset-4 bg-accent/5 rounded-full blur-3xl"></div>
              <img
                src="https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=900&q=80"
                alt="Professional equipment rental"
                className="relative w-full max-h-[75vh] object-contain drop-shadow-2xl"
              />
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

