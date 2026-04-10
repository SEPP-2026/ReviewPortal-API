"use client";

import Link from "next/link";
import {
  Phone,
  Mail,
  MapPin,
  Clock,
  Facebook,
  Twitter,
  Instagram,
  Linkedin,
  ArrowRight,
} from "lucide-react";

const FOOTER_LINKS = {
  equipment: [
    { name: "Construction Equipment", href: "/equipment?category=construction" },
    { name: "Landscaping Tools", href: "/equipment?category=landscaping" },
    { name: "Plumbing Equipment", href: "/equipment?category=plumbing" },
    { name: "Electrical Tools", href: "/equipment?category=electrical" },
    { name: "Decorating Tools", href: "/equipment?category=decorating" },
    { name: "Cleaning Equipment", href: "/equipment?category=cleaning" },
  ],
  company: [
    { name: "About Us", href: "/about" },
    { name: "Our Services", href: "/services" },
    { name: "Pricing Plans", href: "/pricing" },
    { name: "Customer Reviews", href: "/reviews" },
    { name: "Contact Us", href: "/contact" },
    { name: "FAQ", href: "/faq" },
  ],
  support: [
    { name: "Help Center", href: "/help" },
    { name: "Terms & Conditions", href: "/terms" },
    { name: "Privacy Policy", href: "/privacy" },
    { name: "Rental Agreement", href: "/rental-agreement" },
    { name: "Damage Policy", href: "/damage-policy" },
  ],
};

export function Footer() {
  return (
    <footer className="bg-dark border-t border-light-gray/10">
      {/* Main Footer */}
      <div className="max-w-7xl mx-auto px-4 py-16">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-12">
          {/* Company Info */}
          <div>
            <Link href="/" className="flex items-center gap-3 mb-6">
              <div className="w-12 h-12 bg-accent rounded-lg flex items-center justify-center">
                <span className="text-black font-bold text-xl">ST</span>
              </div>
              <div>
                <span className="text-white font-bold text-xl">Shelton</span>
                <span className="text-accent font-bold text-xl"> Tool-Hire</span>
              </div>
            </Link>
            <p className="text-gray mb-6 leading-relaxed">
              Your trusted partner for professional tool and equipment rentals.
              Quality machinery for every project, big or small.
            </p>
            <div className="flex gap-4">
              <a
                href="#"
                className="w-10 h-10 bg-black rounded-lg flex items-center justify-center text-gray hover:text-primary hover:bg-black/50 transition-colors duration-200"
              >
                <Facebook className="w-5 h-5" />
              </a>
              <a
                href="#"
                className="w-10 h-10 bg-black rounded-lg flex items-center justify-center text-gray hover:text-primary hover:bg-black/50 transition-colors duration-200"
              >
                <Twitter className="w-5 h-5" />
              </a>
              <a
                href="#"
                className="w-10 h-10 bg-black rounded-lg flex items-center justify-center text-gray hover:text-primary hover:bg-black/50 transition-colors duration-200"
              >
                <Instagram className="w-5 h-5" />
              </a>
              <a
                href="#"
                className="w-10 h-10 bg-black rounded-lg flex items-center justify-center text-gray hover:text-primary hover:bg-black/50 transition-colors duration-200"
              >
                <Linkedin className="w-5 h-5" />
              </a>
            </div>
          </div>

          {/* Equipment Links */}
          <div>
            <h4 className="text-white font-bold text-lg mb-6">Our Equipment</h4>
            <ul className="space-y-3">
              {FOOTER_LINKS.equipment.map((link) => (
                <li key={link.name}>
                  <Link
                    href={link.href}
                    className="text-gray hover:text-primary transition-colors duration-200 flex items-center gap-2 group"
                  >
                    <ArrowRight className="w-4 h-4 opacity-0 -ml-6 group-hover:opacity-100 group-hover:ml-0 transition-all duration-200" />
                    {link.name}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Company Links */}
          <div>
            <h4 className="text-white font-bold text-lg mb-6">Company</h4>
            <ul className="space-y-3">
              {FOOTER_LINKS.company.map((link) => (
                <li key={link.name}>
                  <Link
                    href={link.href}
                    className="text-gray hover:text-primary transition-colors duration-200 flex items-center gap-2 group"
                  >
                    <ArrowRight className="w-4 h-4 opacity-0 -ml-6 group-hover:opacity-100 group-hover:ml-0 transition-all duration-200" />
                    {link.name}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Contact Info */}
          <div>
            <h4 className="text-white font-bold text-lg mb-6">Contact Us</h4>
            <ul className="space-y-4">
              <li className="flex items-start gap-3">
                <MapPin className="w-5 h-5 text-accent mt-1 shrink-0" />
                <span className="text-gray">
                  123 Industrial Park Road
                  <br />
                  Shelton, CT 06484
                </span>
              </li>
              <li className="flex items-center gap-3">
                <Phone className="w-5 h-5 text-accent shrink-0" />
                <a
                  href="tel:+1234567890"
                  className="text-gray hover:text-primary transition-colors duration-200"
                >
                  +1 (234) 567-890
                </a>
              </li>
              <li className="flex items-center gap-3">
                <Mail className="w-5 h-5 text-accent shrink-0" />
                <a
                  href="mailto:info@sheltontoolhire.com"
                  className="text-gray hover:text-primary transition-colors duration-200"
                >
                  info@sheltontoolhire.com
                </a>
              </li>
              <li className="flex items-start gap-3">
                <Clock className="w-5 h-5 text-accent mt-1 shrink-0" />
                <span className="text-gray">
                  Mon - Sat: 7:00 AM - 6:00 PM
                  <br />
                  Sunday: Closed
                </span>
              </li>
            </ul>
          </div>
        </div>
      </div>

      {/* Bottom Bar */}
      <div className="border-t border-light-gray/10">
        <div className="max-w-7xl mx-auto px-4 py-6">
          <div className="flex flex-col md:flex-row justify-between items-center gap-4">
            <p className="text-gray text-sm">
              © 2026 Shelton Tool-Hire. All rights reserved.
            </p>
            <div className="flex gap-6 text-sm">
              {FOOTER_LINKS.support.slice(0, 3).map((link) => (
                <Link
                  key={link.name}
                  href={link.href}
                  className="text-gray hover:text-primary transition-colors duration-200"
                >
                  {link.name}
                </Link>
              ))}
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}
