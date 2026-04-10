"use client";

import Link from "next/link";
import { Check, ArrowRight } from "lucide-react";

const FEATURES = [
  "24/7 Customer Assistance",
  "High Quality Equipment",
  "Latest Technology",
  "Custom Solutions",
];

export function AboutSection() {
  return (
    <section className="py-24 bg-gradient-to-b from-white to-[#F8F8F8]">
      <div className="max-w-7xl mx-auto px-4">
        {/* Large Heading */}
        <h2 className="text-3xl md:text-4xl lg:text-5xl font-extrabold text-[#111111] max-w-4xl mb-20 leading-[1.2]">
          Everything you need for your project,{" "}
          <span className="text-accent">delivered when you need it</span>
        </h2>

        {/* Three Column Layout */}
        <div className="grid lg:grid-cols-3 gap-12 items-start">
          {/* Left Column */}
          <div>
            <p className="text-[#666666] text-lg leading-relaxed mb-10">
              Our fleet of professional-grade equipment is maintained to the
              highest standards. Whether you need tools for a few hours or heavy
              machinery for weeks, we have flexible solutions to match your
              project needs and budget.
            </p>

            {/* Stats Card */}
            <div className="bg-gradient-to-br from-accent/10 to-accent/5 border-2 border-accent/20 rounded-3xl p-10">
              <div className="text-7xl font-extrabold text-accent mb-3">35+</div>
              <div className="text-[#111111] text-base font-bold uppercase tracking-wider">
                Years of Experience
              </div>
              <p className="text-[#666666] text-sm mt-2">Serving thousands of satisfied customers</p>
            </div>
          </div>

          {/* Center Column — Image */}
          <div className="rounded-3xl overflow-hidden shadow-2xl">
            <img
              src="https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=600&h=700&fit=crop&q=80"
              alt="Professional equipment operator"
              className="w-full h-[500px] object-cover hover:scale-105 transition-transform duration-700"
            />
          </div>

          {/* Right Column */}
          <div>
            <p className="text-[#666666] text-lg leading-relaxed mb-10">
              From the moment you contact us to the final equipment pickup, our
              team ensures a seamless rental experience. Expert advice, on-time
              delivery, and round-the-clock support are part of every rental.
            </p>

            {/* Checklist Card */}
            <div className="bg-white border-2 border-gray-200 rounded-3xl p-8 mb-8 shadow-lg">
              <h3 className="text-xl font-bold text-[#111111] mb-6">What You Get</h3>
              <ul className="space-y-4">
                {FEATURES.map((feature) => (
                  <li
                    key={feature}
                    className="flex items-center gap-4"
                  >
                    <div className="w-8 h-8 bg-accent/10 rounded-full flex items-center justify-center shrink-0">
                      <Check className="w-5 h-5 text-accent" />
                    </div>
                    <span className="text-[#111111] font-semibold text-base">{feature}</span>
                  </li>
                ))}
              </ul>
            </div>

            <Link
              href="/services"
              className="inline-flex items-center gap-2 bg-accent hover:bg-[#C97F00] text-black font-bold px-8 py-4 rounded-xl transition-all duration-200 shadow-lg hover:shadow-xl hover:scale-105"
            >
              Learn More About Us
              <ArrowRight className="w-5 h-5" />
            </Link>
          </div>
        </div>
      </div>
    </section>
  );
}
