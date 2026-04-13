"use client";

interface PostContentProps {
  text: string;
  image?: string;
}

export function PostContent({ text, image }: PostContentProps) {
  return (
    <div className="mt-3">
      <p className="text-white text-[15px] leading-relaxed whitespace-pre-wrap">
        {text}
      </p>
      {image && (
        <div className="mt-3 rounded-2xl overflow-hidden border border-light-gray/20">
          <img
            src={image}
            alt="Post image"
            className="w-full h-auto object-cover max-h-[500px]"
          />
        </div>
      )}
    </div>
  );
}
