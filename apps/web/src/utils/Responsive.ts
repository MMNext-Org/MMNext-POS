export const useMediaQuery = (breakpoint: number) => {
  const [isMobile, setIsMobile] = useState<boolean>(false);

  useEffect(() => {
    const mediaQuery = window.matchMedia(`(max-width: ${breakpoint}px)`);
    
    const handleChange = (event: MediaQueryListEvent) => {
      setIsMobile(event.matches);
    };
    
    // Set initial value
    setIsMobile(mediaQuery.matches);
    
    // Add event listener
    mediaQuery.addEventListener('change', handleChange);
    
    // Cleanup
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, [breakpoint]);

  return isMobile;
};