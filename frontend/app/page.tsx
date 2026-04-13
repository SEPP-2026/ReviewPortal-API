import { Hero } from "@/components/sections/Hero";
import { AboutSection } from "@/components/sections/AboutSection";
import { Categories } from "@/components/sections/Categories";
import { FeaturedEquipment } from "@/components/sections/FeaturedEquipment";
import { Testimonials } from "@/components/sections/Testimonials";
import { CTASection } from "@/components/sections/CTASection";

export default function Home() {
  return (
    <>
      <Hero />
      <AboutSection />
      <Categories />
      <FeaturedEquipment />
      <Testimonials />
      <CTASection />
    </>
  );
}

