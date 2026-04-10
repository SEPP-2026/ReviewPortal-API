"use client";

interface PostHeaderProps {
  profileImage: string;
  name: string;
  handle: string;
  timeAgo: string;
}

export function PostHeader({
  profileImage,
  name,
  handle,
  timeAgo,
}: PostHeaderProps) {
  return (
    <div className="flex items-center gap-3">
      <img
        src={profileImage}
        alt={name}
        className="w-12 h-12 rounded-full object-cover"
      />
      <div className="flex flex-col">
        <div className="flex items-center gap-1">
          <span className="font-bold text-white hover:underline cursor-pointer">
            {name}
          </span>
        </div>
        <div className="flex items-center gap-1 text-gray">
          <span>@{handle}</span>
          <span>·</span>
          <span className="hover:underline cursor-pointer">{timeAgo}</span>
        </div>
      </div>
    </div>
  );
}
